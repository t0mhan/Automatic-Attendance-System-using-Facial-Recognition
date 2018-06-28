# Automatic-Attendance-System-using-Facial-Recognition

Is an Integrated Solution that allows you to have thorough attendance review and eliminate duplicate data entry and errors in time and attendance entries. This application is user-friendly and flexible which helps you to track attendance of the students effectively.

## KEY FEATURES
- Auto Attendance system Using Real Time Face Recognition
- Face Trainer
- Mouth Detection
- Nose Detection
- Eyes Detection

## Why to use this application:
- avoid manual effort and which is time consuming
- it is tedious to keep records in bulk
- reduce the chances of fake attendance which is high in paper-pen attendance
- using automated system, paperwork will be reduced and it will save time and environment

## Target Users
- Universities
- Offices and Industries (future scenarios)

## Technology Used
- OpenCV (for image processing ) 
- WPF (for user interface)
- C# (for implementing business logic)
- Entity Framework (for database management)

## How OpenCV Works
- Some facial recognition algorithms identify faces by extracting landmarks, or features, from an image of the subject's face. For example, an algorithm may analyze the relative position, size, and/or shape of the eyes, nose, cheekbones, and jaw. These features are then used to search for other images with matching features. Other algorithms normalize a gallery of face images and then compress the face data, only saving the data in the image that is useful for face detection. A probe image is then compared with the face data. One of the earliest successful systems is based on template matching techniques applied to a set of salient facial features, providing a sort of compressed face representation. Recognition algorithms can be divided into two main approaches, geometric, which looks at distinguishing features, or photometric, which is a statistical approach that distill an image into values and comparing the values with templates to eliminate variances. Popular recognition algorithms include Principal Component Analysis with eigenface, Linear Discriminate Analysis, Elastic Bunch Graph Matching fisherface, the Hidden Markov model, and the neuronal motivated dynamic link matching.

## Login Credentials
- Login id : **12345678**
- Password : **admin(all small)**

## Installation Guide
### The Basic Requirements 
- Emgucv Libraries (which you can find in presentSir/Emgucv Libraries) already provided in the project.
- Visual studio 2017
- Entity framework 6 for visual studio
1. Go to Tools >> NuGet Package Manager, and click “Manage NuGet Packages for Solution”.
2. Now, the NuGet – Solution window will open. Select "Browse" in NuGet Solution and enter “entity framework core” in the search box. We get many lists after entering the keyword in the search box, the list looks like below screenshot. Locate “Microsoft.EntityFrameworkCore”.

3. Here, we are going to installing ”Microsoft.EntityFrameworkCore”. It is used to access the data for all different types of back-ends. Now, select “Microsoft.EntityFrameworkCore” and select cthe project check box that we are going to install. Then, click the Install button.
As with any c# library there are some essential DLL’s that need referencing within your project. Start a new c# Windows Form Application and call it what you like. All DLL mentioned are in the EMGU extraction Folder\bin you will need to remember this. To start with you need to reference 3 EMGU DLL’s. 
Emgu.CV.dll
Emgu.CV.UI.dll 
Emgu.Util.dll  

