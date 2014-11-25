
GesturePak 2.0!!
================

GesturePak 2.0 is greatly simplified from version 1.0 This version uses KinectTools,
an abstraction over the complex goo required to process body and color frames. That
makes the code in the GestureRecorder app very easy to understand.

GesturePak 2.0 is 100% C# code. The GestureObjects code was ported from VB.NET.

No speech recognition is required. Before you create a gesture you estimate how
many seconds the gesture will take to perform. When you click record you will see
a "countdown," large numbers superimposed over the video (yes, video) image. After
10 seconds, you perform your gesture. The countdown restarts from 5 (or however many
seconds you have given yourself). When it reaches zero you are prompted to save your
gesture to a file.

The edit screen immediately shows. This is where you define the gesture's parameters.

You will see the stick-figure representation of your body performing the gesture.
All frames in those 5 (or more) seconds are captured.

Use the mouse wheel to "scrub" through the frames of the gesture. Trim the unwanted
frames at the start and end of the gesture. Click on the joints that you want to track.
When you come to a frame that you want to match against (in v1 we called these 'poses')
click the "Match" button. You can click the "Animate" button to animate through all of 
the frames. If you only want to see the frames you've matched against, click the 
"Matched Frames Only" checkbox. You can also enter the Fudge Factor, turn on tracking
of the "Hand States" (open, closed, and pointing), and select the axes to track (x, y, z).
Click the "Test" button to test your gesture.

If you'd like to test ALL of the gestures in the \\Documents\\GesturePak folder (where
gestures are saved by default), run the GestureTester project. This app is a great
sample of how you can match against gestures in your own apps.
