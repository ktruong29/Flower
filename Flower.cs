/*******************************************************************************
 *Author: Kien Truong
 *Program: Flower
 ******************************************************************************/
using System.Drawing.Drawing2D;
using System;
using System.Drawing;
using System.Timers;
using System.Windows.Forms;
using System.Collections.Generic;

public class Flower: Form
{
  private const int maxFormWidth  = 1920;
  private const int maxFormHeight = 1080;
  private const int minFormWidth  = 640;
  private const int minFormHeight = 360;
  Size maxFrameSize = new Size(maxFormWidth,maxFormHeight);
  Size minFrameSize = new Size(minFormWidth,minFormHeight);

  private const int topPanelHeight    = 50;
  private const int bottomPanelHeight = 110;

  private const int control_region_height = 130;     //This region holds buttons and input boxes -- if any.
  private int graphic_area_height;  //Will be set equal to form_height - conrol_region_height

  //Declaring the 3 important numbers: linear speed, refrest rate, and motion rate
  private const double linear_velocity = 300.0; //pixels per second.
                                                //These are not math units; these are graphical pixels.

  //Declare rates for the clocks
  private const double refresh_rate = 58.5; //Hertz: How many times per second the bitmap is copied to the visible surface.
  private const double spiral_rate  = 48.0; //Hertz: How many times per second the coordinates of the brush are updated.

  //Declare scale factor: for example, '30' means "30 to 1" or "30 pixels represent 1 mathematical unit".
  private const double scale_factor = 80.0;

  //Declare a name for the distance the drawing brush travels in one tic of the spiral_clock.  For example, if the velocity of
  //the brush drawing the spiral is 120 pixels/sec and the clock tics at the rate of 30 Hz, then the brush moves a distance of
  //120/30 = 4 pixels per tic.  The concept here is the "pixel_distance_traveled_in_one_tic" is the same as "brush motion speed"
  //with units in pixels per second.
  private const double pixel_distance_traveled_in_one_tic = linear_velocity/spiral_rate;  //The units are in pixels.
  //Convert pixel_distance to mathematical distance.
  private const double mathematical_distance_traveled_in_one_tic = pixel_distance_traveled_in_one_tic/scale_factor;

  //Declare the polar origin in C# coordinates
  private int polar_origen_x;   // = (int)System.Math.Round(form_width/2.0);
  private int polar_origen_y;   // = (int)System.Math.Round(graphic_area_height/2.0);

  //Declare the width of the spiral curve.  Be aware that in the method Update_the_graphic_area the value will be divided by 2
  //using integer division.  Therefore the width of the curve should be at least 2.
  //division; namely: spiral_width/2 in the method Update_the_graphic_area.
  private const int spiral_width = 2;  //The value 1 results in a very thin barely visible curve.

  //Declare variables for polar coordinate system
  private int unit_circle_radius = (int)System.Math.Round(scale_factor);

  private const double initial_radius = 0.0;
  private const double distance_between_rings = 1.0;
  private const double b_coefficient = distance_between_rings/System.Math.PI/2.0;
  private double t = 0.0; //t is angle between polar axis and the ray emanating from the pole to the pen; t≥0.
  private double x;       //Cartesian x-coordinate of the point drawing the spiral trace
  private double y;       //Cartesian x-coordinate of the point drawing the spiral trace

  //Variables for the scaled description of the Archimedean spiral
  private double x_scaled_double;
  private double y_scaled_double;

  //Variables detecting time to stop execution
  private bool spiral_too_large_vertically   = false;
  private bool spiral_too_large_horizontally = false;

  //Variables for drawing on the bitmap
  private int x_scaled_int;
  private int y_scaled_int;

  private double magnitude_of_tangent_vector_squared;
  private double magnitude_of_tangent_vector;

  private const String welcome_message = "Curve of r=cos(2t) programmed by Kien Truong";
  private System.Drawing.Font welcome_style = new System.Drawing.Font("TimesNewRoman",24,FontStyle.Regular);
  private Brush welcome_paint_brush = new SolidBrush(System.Drawing.Color.Black);
  private Point welcome_location;   //Will be initialized in the constructor.

  private Button goButton    = new Button();
  private Button exitButton  = new Button();
  private Button pauseButton = new Button();
  private Label  xLabel      = new Label();
  private Label  yLabel      = new Label();
  private Label  xCoordinate = new Label();
  private Label  yCoordinate = new Label();

  private int formWidth;
  private int formHeight;

  //Declare clocks
  private static System.Timers.Timer graphic_area_refresh_clock = new System.Timers.Timer();
  private static System.Timers.Timer spiral_clock = new System.Timers.Timer();
  private enum Spiral_clock_state_type{Begin,Ticking,Paused};
  private Spiral_clock_state_type spiral_state = Spiral_clock_state_type.Begin;

  //Instruments
  private Pen bic = new Pen(Color.Black,1); //The bic pen has a thickness of 1 pixel.

  //Declare pointers to the visible graphical area and and to the bitmap area of memory.
  private System.Drawing.Graphics pointer_to_graphic_surface;
  private System.Drawing.Bitmap pointer_to_bitmap_in_memory;  // = new Bitmap(form_width,form_height,System.Drawing.Imaging.PixelFormat.Format24bppRgb);

  //The value of the next variable is set by the function Manage_spiral_clock, which is the event handler of Spiral_clock.
  double elapsed_time_between_updates_of_spiral_coordinates;                  //Units are milliseconds
  double elapsed_time_between_updates_of_spiral_coordinates_rounded;          //Units are milliseconds
  double elapsed_time_between_updates_of_spiral_coordinates_rounded_seconds;  //Units are whole seconds

 public Flower()
 {//Set the size of the user interface box.
  formWidth  = (maxFormWidth+minFormWidth)/2;
  formHeight = (maxFormHeight+minFormHeight)/2;
  graphic_area_height = formHeight - control_region_height - topPanelHeight;
  Size = new Size(formWidth,formHeight);

  MaximumSize = maxFrameSize;
  MinimumSize = minFrameSize;

  //Set the location of the polar origin in C# coordinates
  polar_origen_x = (int)System.Math.Round(formWidth/2.0);
  polar_origen_y = (int)System.Math.Round(graphic_area_height/2.0);

  //Set initial values for the flower
  t = 0.0;
  x = 400*System.Math.Cos(2*t)*System.Math.Cos(t);
  y = 400*System.Math.Cos(2*t)*System.Math.Sin(t);

  //Initialize text strings
  Text = "Falling Apples by Kien Truong";
  System.Console.WriteLine("Form_width = {0}, Form_height = {1}.", Width, Height);
  pauseButton.Text    = "Pause";
  goButton.Text       = "Go";
  exitButton.Text     = "Exit";
  xLabel.Text         = "x = ";
  yLabel.Text         = "y = ";
  xCoordinate.Text    = "0.0";
  yCoordinate.Text    = "0.0";

  //Set sizes
  pauseButton.Size    = new Size(100,40);
  goButton.Size       = new Size(100,40);
  exitButton.Size     = new Size(100,40);
  xLabel.Size         = new Size(50,20);
  yLabel.Size         = new Size(50,20);
  xCoordinate.Size    = new Size(100,20);
  yCoordinate.Size    = new Size(100,20);

  //Set locations
  pauseButton.Location = new Point(600,600);
  goButton.Location    = new Point(50,600);
  exitButton.Location  = new Point(1000,600);
  xLabel.Location      = new Point(200,600);
  yLabel.Location      = new Point(200,630);
  xCoordinate.Location = new Point(260,600);
  yCoordinate.Location = new Point(260,630);

  //Set colors
  this.BackColor          = Color.Blue;
  goButton.BackColor      = Color.White;
  pauseButton.BackColor   = Color.White;
  exitButton.BackColor    = Color.White;
  xLabel.BackColor        = Color.White;
  yLabel.BackColor        = Color.White;
  xCoordinate.BackColor   = Color.White;
  yCoordinate.BackColor   = Color.White;

  //Add controls to the form
  Controls.Add(pauseButton);
  Controls.Add(goButton);
  Controls.Add(exitButton);
  Controls.Add(xLabel);
  Controls.Add(yLabel);
  Controls.Add(xCoordinate);
  Controls.Add(yCoordinate);

  welcome_location = new Point(Width/2-250,8);

  //Register the event handler.  In this case each button has an event handler, but no other
  //controls have event handlers.
  pauseButton.Enabled     = true;
  goButton.Enabled        = true;
  exitButton.Enabled      = true;

  //Set up the refresh clock
  graphic_area_refresh_clock.Enabled = false;
  graphic_area_refresh_clock.Elapsed += new ElapsedEventHandler(Update_the_graphic_area);

  //Set up the spiral clock.
  spiral_clock.Enabled = false;
  spiral_clock.Elapsed += new ElapsedEventHandler(Update_the_position_of_the_spiral);

  //Use extra memory to make a smooth animation.
  DoubleBuffered = true;

  pointer_to_bitmap_in_memory = new Bitmap(formWidth,graphic_area_height,System.Drawing.Imaging.PixelFormat.Format24bppRgb);
  pointer_to_graphic_surface  = Graphics.FromImage(pointer_to_bitmap_in_memory);

  initialize_bitmap();

  //Start refresh clock running
  Start_graphic_clock(refresh_rate);

  //Make sure this graphical window appears in the center of the monitor.
  CenterToScreen();

  pauseButton.Click += new EventHandler(pause);
  goButton.Click    += new EventHandler(start);
  exitButton.Click  += new EventHandler(stoprun);  //The '+' is required.
}//End of constructor Flower

 protected void initialize_bitmap()
 {
   //Explanation: The method contain 5 statements that perform a Draw action on "pointer_to_graphic_surface".  Each one of
    //those statements is an output action onto the bitmap area in memory.  The name "pointer_to_graphic_surface" seems counter
    //intuitive, but in fact the output does go to a block in memory.
    Font numeric_label_font = new System.Drawing.Font("Arial",8,FontStyle.Regular);
    Brush numeric_label_brush = new SolidBrush(System.Drawing.Color.Black);
    pointer_to_graphic_surface.Clear(System.Drawing.Color.White);
    double numeric_label = 0.0; //Declare a variable to assist in placing numeric labels on the polar axis.

    //The next few statements provide feedback for the programmer.  These statements should be removed by the next programmer.
    System.Console.WriteLine("formWidth = {0}, form_height = {1}, graphic_area_height = {2} .",
                              formWidth, formHeight, graphic_area_height);
    System.Console.WriteLine("form_height/2 = {0}, scale_factor = {1}, form_height/2/scale_factor = {2} .",
                              formWidth/2, scale_factor, formHeight/scale_factor/2);
    System.Console.WriteLine("polar_origen_x = {0}, polar_origen_y = {1}.", polar_origen_x, polar_origen_y);
    //The radial distance between consecutive polar circles is 1 mu (mathematical unit) = scale_factor.
    //Therefore, the cartesian coordinates of the point where the inner-most concentric circle crosses the polar axis is
    //(polar_origen_x + unit_circle_radius,polar_origen_y).
    System.Console.WriteLine("Cartesian x-coordinate of intersection of unit circle and polar axis = {0}",polar_origen_x+unit_circle_radius);
    System.Console.WriteLine("Cartesian y-coordinate of intersection of unit circle and polar axis = {0}",polar_origen_y);

    //Draw concentric polar circles as viewing guides
    bic.DashStyle = DashStyle.Dot;
    bic.Width = 1;
    for(int c = 1; c < formHeight/scale_factor/2; c++)
            pointer_to_graphic_surface.DrawEllipse(bic,polar_origen_x-c*unit_circle_radius,polar_origen_y-c*unit_circle_radius,
                                               2*c*unit_circle_radius,2*c*unit_circle_radius);

    //Draw the polar axis
    bic.DashStyle = DashStyle.Solid;  //Dash, Dot, DashDot, DashDotDot
    bic.Width = 1;  //The bic pen will draw with a width = 2 pixels
    pointer_to_graphic_surface.DrawLine(bic,formWidth/2,graphic_area_height/2,formWidth,graphic_area_height/2);

    //Draw labels along the polar axis using a loop
    numeric_label = 0.0;
    while(numeric_label*scale_factor*2.0 < (float)formWidth)
    {  pointer_to_graphic_surface.DrawString(String.Format("{0:0.0}",numeric_label),numeric_label_font,numeric_label_brush,
                                             polar_origen_x+(int)System.Math.Round(numeric_label*scale_factor-10.0),
                                             polar_origen_y+2);
       numeric_label = numeric_label + 1.0;  //Increment the loop control variable
    }//End of while

    //Draw a pseudo-panel as a thin long rectangle along the bottom of the form.
    pointer_to_graphic_surface.FillRectangle(Brushes.Yellow,0,formHeight-control_region_height,formWidth,control_region_height);
 }

 protected void stoprun(Object sender, EventArgs events)
 {
   Close();
 }//End of stoprun

 protected void pause(Object sender, EventArgs events)
 {
   if(pauseButton.Text == "Pause")
   {
     spiral_clock.Enabled = false;
     graphic_area_refresh_clock.Enabled = false;
     pauseButton.Text = "Resume";
   }
   else
   {
     spiral_clock.Enabled = true;
     graphic_area_refresh_clock.Enabled = true;
     pauseButton.Text = "Pause";
   }
 }

 protected void start(Object sender, EventArgs events)
 {
     double local_spiral_update_rate = spiral_rate;
     //In the next statement don't allow the spiral to update at a rate slower than 1.0 Hz
     if(local_spiral_update_rate < 1.0) local_spiral_update_rate = 1.0;
     elapsed_time_between_updates_of_spiral_coordinates = 1000.0/local_spiral_update_rate;  //Units are milliseconds
     elapsed_time_between_updates_of_spiral_coordinates_rounded = System.Math.Round(elapsed_time_between_updates_of_spiral_coordinates);
     spiral_clock.Interval = (int)elapsed_time_between_updates_of_spiral_coordinates_rounded;
     elapsed_time_between_updates_of_spiral_coordinates_rounded_seconds = elapsed_time_between_updates_of_spiral_coordinates_rounded/1000.0;
     spiral_clock.Enabled = true;
     graphic_area_refresh_clock.Enabled = true;
     System.Console.WriteLine("Begin case finished executing");
 }//End of start

 protected void Start_graphic_clock(double refreshrate)
 {
    double elapsedtimebetweentics;
    if(refreshrate < 1.0)
    {
      refreshrate = 1.0;  //Do not allow updates slower than 1 hertz.
    }
    elapsedtimebetweentics = 1000.0/refreshrate;  //elapsed time between tics has units milliseconds
    graphic_area_refresh_clock.Interval = (int)System.Math.Round(elapsedtimebetweentics);
    graphic_area_refresh_clock.Enabled = true;  //Start this clock ticking.
    System.Console.WriteLine("Start_graphic_clock has terminated.");
 }//End of Start_graphic_clock

 protected void Update_the_position_of_the_spiral(System.Object sender,ElapsedEventArgs an_event)
 {
    magnitude_of_tangent_vector_squared = 500-(300*System.Math.Cos(4*t));
    magnitude_of_tangent_vector = System.Math.Sqrt(magnitude_of_tangent_vector_squared);
    t = t + mathematical_distance_traveled_in_one_tic/magnitude_of_tangent_vector;
    // t = t + 0.008;
    x = 2*System.Math.Cos(2*t)*System.Math.Cos(t);
    y = 2*System.Math.Cos(2*t)*System.Math.Sin(t);

    x_scaled_double = scale_factor * x;
    y_scaled_double = scale_factor * y;

    xCoordinate.Text = (System.Math.Round(x,8)).ToString();
    yCoordinate.Text = (System.Math.Round(y,8)).ToString();
 }//End of method Update_the_position_of_the_spiral

 protected override void OnPaint(PaintEventArgs ee)
 {
   Graphics lights = ee.Graphics;
   Pen blackPen = new Pen(Color.Black, 3);

   lights.FillRectangle(Brushes.Green,0,0,Width,topPanelHeight);
   lights.DrawString(welcome_message,welcome_style,welcome_paint_brush,welcome_location);

   // lights.FillRectangle(Brushes.Brown,0,410,Width,topPanelHeight+180);
   lights.FillRectangle(Brushes.Yellow,0,590,Width,topPanelHeight+130);
   lights.DrawImage(pointer_to_bitmap_in_memory,0,topPanelHeight,formWidth,graphic_area_height);

   base.OnPaint(ee);
 }//END protected override void OnPaint(PaintEventArgs ee)

 protected void Update_the_graphic_area(System.Object sender, ElapsedEventArgs even)    //Activated by the refresh clock.
 {
   x_scaled_int = (int)System.Math.Round(x_scaled_double);
   y_scaled_int = (int)System.Math.Round(y_scaled_double);

   if(0 <= polar_origen_y-y_scaled_int-spiral_width/2 && polar_origen_y-y_scaled_int-spiral_width/2 < graphic_area_height)
   {
     if(0 <= polar_origen_x+x_scaled_int-spiral_width/2 && polar_origen_x+x_scaled_int-spiral_width/2 < formWidth)
     {
       pointer_to_graphic_surface.FillEllipse(Brushes.Red,
                                       polar_origen_x+x_scaled_int-spiral_width/2,
                                       polar_origen_y-y_scaled_int-spiral_width/2,  //There is a subtraction here because the y-axis is upside down.
                                       spiral_width,
                                       spiral_width);
     }
     else
     {
           spiral_too_large_horizontally = true;
     }
   }
   //The if-condition below does not allow the spiral to write outside of the left or right boundaries of the graphic area.
   else
   {
     spiral_too_large_vertically = true;
   }

   Invalidate();  //This function actually calls OnPaint.  Yes, that is true.

   if(spiral_too_large_horizontally && spiral_too_large_vertically)  //It is time to stop execution
   {
     graphic_area_refresh_clock.Enabled = false;  //Stop refreshing the graphic area
      spiral_clock.Enabled = false;
      goButton.Enabled = false;
      System.Console.WriteLine("The graphical area is no longer refreshing.  You may close the window.");
   }
 }//End of Update_the_graphic_area

}//End of class Flower
