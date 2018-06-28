using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Interop;

namespace AttendanceSystem
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Image<Bgr, Byte> currentFrameRecognition; //to open the camera 
        Emgu.CV.Capture grabber, grabberRecognition;
        HaarCascade faceRecognition;
        Image<Gray, byte> result = null;
        List<Image<Gray, byte>> trainingImages = new List<Image<Gray, byte>>();
        List<string> labelsRecognition = new List<string>();
        MCvFont font = new MCvFont(FONT.CV_FONT_HERSHEY_TRIPLEX, 0.5d, 0.5d);
        List<string> NamePersons = new List<string>();
        string name, names = null;
        int t, contTrain, numLabels;
        bool imageCaptured = false;
        private HashSet<string> FacesAlreadyDetected = new HashSet<string>();
        Image<Bgr, byte> currentFrame;
        Image<Gray, byte> gray, resultFace, resultEyes, resultMouth, resultNose, trainedFace = null, trainedEyes = null, trainedMouth = null, trainedNose = null; //initializing as an empty object  
        //initializing hharcascade for face detection (detects in order)
        HaarCascade face, eyes, mouth, nose; //detection by face, eyes,nose,mouth
        //initializing faces and name storage array 
        List<Image<Gray, byte>> detectedFace = new List<Image<Gray, byte>>();
        List<Image<Gray, byte>> detectedEyes = new List<Image<Gray, byte>>();
        List<Image<Gray, byte>> detectedMouth = new List<Image<Gray, byte>>();
        List<Image<Gray, byte>> detectedNose = new List<Image<Gray, byte>>();
        List<string> labels = new List<string>();
        int faceCount, eyeCount, mouthCount, noseCount = 0; //set counters to 0 and increment it 1 when face is found 

        public MainWindow()
        {
            face = new HaarCascade("haarcascade-frontalface-default.xml");
            //Load haarcascades for eye detection
            eyes = new HaarCascade("haarcascade_mcs_eyepair_big.xml");
            //Load haarcascades for mouth detection
            mouth = new HaarCascade("mouth.xml");
            //Load haarcascades for nose detection
            nose = new HaarCascade("nose.xml");

            InitializeComponent();
            datePicker.SelectedDate = DateTime.Now.Date;
            faceRecognition = new HaarCascade("haarcascade-frontalface-default.xml");
            TBx_Nose.Visibility = TBx_Eyes.Visibility = TBx_Face.Visibility = TBx_Mouth.Visibility = BTn_AddStudent.Visibility = BTn_CaptureAgain.Visibility = Visibility.Collapsed;
            try
            {
                //Load previous trainned faces of students and their names
                string Labelsinfo = File.ReadAllText(Environment.CurrentDirectory + "/TrainedFaces/TrainedNames.txt");
                string[] Labels = Labelsinfo.Split('%');
                numLabels = Convert.ToInt16(Labels[0]);
                contTrain = numLabels;
                string LoadFaces;
                for (int tf = 1; tf < numLabels + 1; tf++)
                {
                    LoadFaces = "face" + Labels[tf] + ".bmp";
                    trainingImages.Add(new Image<Gray, byte>(Environment.CurrentDirectory + "/TrainedFaces/" + LoadFaces));
                    //make a list of string
                    labelsRecognition.Add(Labels[tf]);
                }
            }

            catch (Exception e)
            {
                System.Windows.MessageBox.Show("Press OK to proceed!");
            }
        }

        private void BTn_Detect_Click(object sender, RoutedEventArgs e)
        {
            grabberRecognition = new Emgu.CV.Capture(1); //when click camera wil be opened
            //initializing the grabber event 
            grabberRecognition.QueryFrame();
            //Now to capture the video 
            ComponentDispatcher.ThreadIdle += new EventHandler(FrameGrabberRecognition); //if the application is idel and the camera is on then call the frame grabber event 
        }

        private void BTn_ViewAttendance_Click(object sender, RoutedEventArgs e)
        {
            LoadGrid();
        }

        private void LoadGrid()
        {
            databaseEntities db = new databaseEntities();
            List<attendance> tableData = db.attendances.ToList();
            dataGrid.ItemsSource = tableData;
        }

        private void BTn_AddStudent_Click(object sender, RoutedEventArgs e)
        {
            StudentDetails(TBx_Matriculation.Text);
        }

        private void BTn_Go_Click(object sender, RoutedEventArgs e)
        {
            LBl_WarningMsg.Content = "";
            if (TBx_Matriculation.Text != "")
            {
                int i;
                if (int.TryParse(TBx_Matriculation.Text, out i))
                {
                    bool hasMat, studExists;
                    string uniStudents = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "/TrainedFaces/UniStudents.txt");
                    string[] arrayUniStudents = uniStudents.Split('%');
                    hasMat = Array.IndexOf(arrayUniStudents, TBx_Matriculation.Text) >= 0;
                    if (hasMat)
                    {
                        string addedStudents = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "/TrainedFaces/TrainedNames.txt");
                        string[] arrayaddedStudents = addedStudents.Split('%');
                        studExists = Array.IndexOf(arrayaddedStudents, TBx_Matriculation.Text) >= 0;
                        LBl_WarningMsg.Content = "";
                        if (!studExists)
                            try
                            {
                                TBx_Nose.Visibility = TBx_Eyes.Visibility = TBx_Face.Visibility = TBx_Mouth.Visibility = BTn_AddStudent.Visibility = BTn_CaptureAgain.Visibility = Visibility.Visible;
                                grabber = new Emgu.CV.Capture(1); //when click camera wil be opened
                                grabber.QueryFrame(); //Now to capture the video 
                                ComponentDispatcher.ThreadIdle += new EventHandler(FrameGrabber); //if the application is idel and the camera is on then call the frame grabber event
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message);
                            }
                        else
                            LBl_WarningMsg.Content = "Student already added in the Attendance System";
                    }
                    else
                        LBl_WarningMsg.Content = "Student is not enrolled in the university";
                }
                else
                    LBl_WarningMsg.Content = "Characters otherthan numbers not allowed!";
            }
            else
                LBl_WarningMsg.Content = "Matriculation number can not be empty!";
        }

        private void BTn_CaptureAgain_Click(object sender, RoutedEventArgs e)
        {
            imageCaptured = false;
            //faceCount = eyeCount = mouthCount = noseCount = 0;
            grabber = new Emgu.CV.Capture(1); //when click camera wil be opened
            //initializing the grabber event 
            grabber.QueryFrame();
            //Now to capture the video 
            ComponentDispatcher.ThreadIdle += new EventHandler(FrameGrabber); //if the application is idel and the camera is on then call the frame grabber event
        }

        public void ExtractFeatures()
        {
            try
            {
                trainedFace = resultFace.Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                trainedEyes = resultEyes.Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                trainedMouth = resultMouth.Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                trainedNose = resultNose.Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);

                //Image box to show the detected faces
                imgBoxFace.Image = trainedFace;
                imgBoxEyes.Image = trainedEyes;
                imgBoxMouth.Image = trainedMouth;
                imgBoxNose.Image = trainedNose;
                imageCaptured = true;
            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.Message);
            }
        }

        public void StudentDetails(string matriculationNumber)
        {
            try
            {
                string Labelsinfo = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "/TrainedFaces/TrainedNames.txt");
                Labelsinfo = Labelsinfo + "%" + matriculationNumber;
                string[] Labels = Labelsinfo.Split('%');
                //NumLabels = Convert.ToInt16(Labels[0]);
                Labels[0] = Convert.ToString(Convert.ToInt16(Labels[0]) + 1);

                //detected faces will be saved into a folder with the name of the person 
                detectedFace.Add(trainedFace);
                detectedEyes.Add(trainedEyes);
                detectedMouth.Add(trainedMouth);
                detectedNose.Add(trainedNose);
                labels.Add(TBx_Matriculation.Text);

                //write to files 
                for (int i = 1; i < detectedFace.ToArray().Length + 1; i++)
                {
                    //save faces to folder with name face(i) i being the name/number of the face detected
                    detectedFace.ToArray()[i - 1].Save(AppDomain.CurrentDomain.BaseDirectory + "/TrainedFaces/face" + matriculationNumber + ".bmp");
                }

                for (int i = 1; i < detectedEyes.ToArray().Length + 1; i++)
                {
                    //save faces to folder with name face(i) i being the name/number of the face detected
                    detectedEyes.ToArray()[i - 1].Save(AppDomain.CurrentDomain.BaseDirectory + "/TrainedFaces/eyes" + matriculationNumber + ".bmp");
                    //Saves name to text file
                }

                for (int i = 1; i < detectedMouth.ToArray().Length + 1; i++)
                {
                    //save faces to folder with name face(i) i being the name/number of the face detected
                    detectedMouth.ToArray()[i - 1].Save(AppDomain.CurrentDomain.BaseDirectory + "/TrainedFaces/mouth" + matriculationNumber + ".bmp");
                }

                for (int i = 1; i < detectedNose.ToArray().Length + 1; i++)
                {
                    //save faces to folder with name face(i) i being the name/number of the face detected
                    detectedNose.ToArray()[i - 1].Save(AppDomain.CurrentDomain.BaseDirectory + "/TrainedFaces/nose" + matriculationNumber + ".bmp");
                    //Saves name to text file
                    File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "/TrainedFaces/TrainedNames.txt", String.Join("%", Labels));
                }
                MessageBox.Show("Images added to Training Data");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void FrameGrabberRecognition(object sender, EventArgs e)
        {
            try
            {
                NamePersons.Add("");
                //Get the current frame form capture device
                currentFrameRecognition = grabberRecognition.QueryFrame().Resize(501, 407, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                //Convert it to Grayscale
                gray = currentFrameRecognition.Convert<Gray, Byte>();
                //Face Detector
                MCvAvgComp[][] facesDetected = gray.DetectHaarCascade(
              faceRecognition,
              1.2,
              2,
              Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING,
              new System.Drawing.Size(20, 20));
                //Action for each element detected
                foreach (MCvAvgComp f in facesDetected[0])
                {
                    t = t + 1;
                    result = currentFrameRecognition.Copy(f.rect).Convert<Gray, byte>().Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                    //draw the face detected in the 0th (gray) channel with blue color
                    currentFrameRecognition.Draw(f.rect, new Bgr(Color.Red), 2);
                    //initialize result,t and gray if (trainingImages.ToArray().Length != 0)
                    {
                        //term criteria against each image to find a match with it, perform different iterations
                        MCvTermCriteria termCrit = new MCvTermCriteria(contTrain, 0.001);
                        //call class by creating object and pass parameters
                        EigenObjectRecognizer recognizer = new EigenObjectRecognizer(
                             trainingImages.ToArray(),
                             labelsRecognition.ToArray(),
                             3000,
                             ref termCrit);
                        //next step is to name find for recognize face
                        name = recognizer.Recognize(result);
                        //now show recognized person name so
                        currentFrameRecognition.Draw(name, ref font, new System.Drawing.Point(f.rect.X - 2, f.rect.Y - 2), new Bgr(Color.LightGreen));//initalize font for the name captured
                    }
                    if (!FacesAlreadyDetected.Contains(name))
                    {
                        SaveToDatabase(name, DateTime.Now);
                        FacesAlreadyDetected.Add(name);
                    }
                    NamePersons[t - 1] = name;
                    NamePersons.Add("");
                }
                t = 0;
                //Names concatenation of persons recognized
                for (int nnn = 0; nnn < facesDetected[0].Length; nnn++)
                {
                    names = names + NamePersons[nnn] + ", ";
                    //System.Windows.MessageBox.Show(NamePersons[nnn]);
                    string test = NamePersons[nnn] + ",";
                }
                //load haarclassifier and previous saved images to find matches
                imgBox_Detected.Image = currentFrameRecognition;
                TBx_DetectedStudent.Text = names;
                names = "";
                NamePersons.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Press Ok to Continue");
            }
        }

        private void SaveToDatabase(string name, DateTime now)
        {
            databaseEntities db = new databaseEntities();
            attendance at = new attendance();
            at.studentid = Convert.ToInt32(name);
            db.attendances.Add(at);
            at.dateandtime = now;
            db.attendances.Add(at);
            db.SaveChanges();
        }

        public void FrameGrabber(object sender, EventArgs e) //Frame grabber event 
        {
            try
            {
                if (imageCaptured == false)
                {
                    //initialize current frame with query grabber which is catching the frame
                    currentFrame = grabber.QueryFrame().Resize(501, 407, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC); //resizing the frame with cubic frame                                                                                                              
                    // Converting image frame to gray scale (image processing) 
                    gray = currentFrame.Convert<Gray, Byte>();

                    // Detecting face by using Haar Classifier 
                    MCvAvgComp[][] facesDetected = gray.DetectHaarCascade(face, 1.2, 1, Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new System.Drawing.Size(20, 20));
                    MCvAvgComp[][] eyesDetected = gray.DetectHaarCascade(eyes, 1.2, 5, Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new System.Drawing.Size(20, 20));
                    MCvAvgComp[][] mouthDetected = gray.DetectHaarCascade(mouth, 1.2, 10, Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new System.Drawing.Size(50, 50));
                    MCvAvgComp[][] noseDetected = gray.DetectHaarCascade(nose, 1.2, 5, Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new System.Drawing.Size(20, 20));
                    //face is name of the haar cascade, giving sizes to the cascade, applying canny pruning on haar classifier 

                    // Checking each frame of image processed by the classifer through ImageBox (video is processed as image frames for face detection), then detect face
                    foreach (MCvAvgComp f in facesDetected[0])
                    {
                        // If face is detected then increment t into 1 = True 
                        faceCount = faceCount + 1;
                        // Copy detected face in a frame name as resultFace (gray.resultFace)
                        resultFace = currentFrame.Copy(f.rect).Convert<Gray, byte>().Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                        // Drawing rectangle around on detected image (face) 
                        currentFrame.Draw(f.rect, new Bgr(System.Drawing.Color.Red), 2);
                    }

                    foreach (MCvAvgComp f in eyesDetected[0])
                    {
                        // If face is detected then increment t into 1 = True 
                        eyeCount = eyeCount + 1;
                        // Copy detected face in a frame name as resultFace (gray.resultFace)
                        resultEyes = currentFrame.Copy(f.rect).Convert<Gray, byte>().Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                        // Drawing rectangle around on detected image 
                        currentFrame.Draw(f.rect, new Bgr(System.Drawing.Color.White), 2);
                    }

                    foreach (MCvAvgComp f in mouthDetected[0])
                    {
                        // If face is detected then increment t into 1 = True 
                        mouthCount = mouthCount + 1;
                        // Copy detected face in a frame name as resultFace (gray.resultFace)
                        resultMouth = currentFrame.Copy(f.rect).Convert<Gray, byte>().Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                        // Drawing rectangle around on detected image 
                        currentFrame.Draw(f.rect, new Bgr(System.Drawing.Color.Green), 2);
                    }

                    foreach (MCvAvgComp f in noseDetected[0])
                    {
                        // If face is detected then increment t into 1 = True 
                        noseCount = noseCount + 1;
                        // Copy detected face in a frame name as resultFace (gray.resultFace)
                        resultNose = currentFrame.Copy(f.rect).Convert<Gray, byte>().Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                        // Drawing rectangle around on detected image 
                        currentFrame.Draw(f.rect, new Bgr(System.Drawing.Color.Blue), 2);
                    }
                    //View current frame in the imported ImageBox
                    imgBox.Image = currentFrame; //current frame = captured from the camera into the imagebox
                    if (faceCount > 0 & eyeCount > 0 & mouthCount > 0 & noseCount > 0)
                    {
                        ExtractFeatures();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Press Ok to Continue");
            }
        }
    }
}
