# Flower
---
The purpose of this program is to have a better understanding of how bitmap works.
OnPaint erases the entire existing graphical image and re-paints a new image.
Writing to a bitmap in memory is a way to preserve the existing graphical image,
then change a few pixels, then re-paint the entire image with the few changes.
The program reserves a section of memory, which is called "bitmap". The program
writes its images onto the bitmap, but not directly onto the graphic area of the
UI. In memory, old things are not overwritten when you output something new to
the bitmap. When the refresh clock tics, OnPaint refreshes the UI graphic area
by copying the bitmap onto the UI.

## Specifications
---
* When the user clicks on "Start" button, the UI will start drawing a 4-leaf flower.
* It keeps track and update the current coordinates.
* When the user clicks on "Pause", the UI freezes and the text string will change
to be "Resume". When the user clicks on "Resume" button, the UI starts executing
at the point where it paused.
* When the user clicks on "Exit", the program will be terminated.

### Math requirements
* For this program, the function that is used is r = cos(2t), which is a 4-leaf flower.
* Converting the equation to regular Cartesian coordinates:
    x(t) = cos(2t) * cos(t)
    y(t) = cos(2t) * sin(t) with 0 <= t <= 2pi
* As the parameter t advances from 0 to 2pi, the dot with coordinates (x(t), y(t))
will draw the flower. Delta_t is an increment for t. Each time the motion clock
tics, delta_t is added to t, and a filled ellipse is drawn at the position(x(t), y(t)).
The filled ellipse has a radius of 1 pix and appears as a single dot.

## Prerequisites
---
* A virtual machine
* Install mcs

## Instruction on how to run the program
---
1. chmod +x build.sh then ./build.sh
2. sh build.sh

Copyright [2019] [Kien Truong]
