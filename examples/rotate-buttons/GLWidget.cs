namespace GtkGL {

	using System;
	using System.Collections;

	using Tao.OpenGl;

	using Gtk;
	using Gdk;

	using gl=Tao.OpenGl.Gl;
	using glu=Tao.OpenGl.Glu;


	public class GLWidget : GLArea {
		
		static System.Int32[] attrlist = {
		    (int)GtkGL._GDK_GL_CONFIGS.Rgba,
		    (int)GtkGL._GDK_GL_CONFIGS.RedSize,1,
		    (int)GtkGL._GDK_GL_CONFIGS.GreenSize,1,
		    (int)GtkGL._GDK_GL_CONFIGS.BlueSize,1,
		    (int)GtkGL._GDK_GL_CONFIGS.DepthSize,1,
		    (int)GtkGL._GDK_GL_CONFIGS.Doublebuffer,
		    (int)GtkGL._GDK_GL_CONFIGS.None,
		};
		
		protected ArrayList GLObjectList;
		
		public void AddGLObject(IGLObject ob)
		{
			ob.Updated += OnExposed;

			GLObjectList.Add(ob);
		}
		
		private void Init()
		{
			// The GL widget is a minimum of 300x300 pixels 
			this.SetSizeRequest(300,300);
			
			// Initialize the GLObjectList
			GLObjectList = new ArrayList();
			
			// Connect some other signals		
			this.ExposeEvent += OnExposed;
			this.Realized += OnRealized;
			this.SizeAllocated += OnSizeAllocated;
			this.ConfigureEvent += OnConfigure;
		}
		
		public GLWidget() : base(attrlist) {
			this.Init();
		}
		
		public GLWidget(System.Int32[] attrList) : base(attrList)
		{
			this.Init();			
		}
		
		// This handler gets fired when the glArea widget is re-sized
		void OnSizeAllocated (object o, Gtk.SizeAllocatedArgs e)
		{
			int height = e.Allocation.Height, width = e.Allocation.Width;
			
			// Avoid devide-by-zero error
			if(height == 0){
				height = 1;
			}
			
			// Set our Viewport size
			gl.glViewport(0, 0, width, height);
			
			gl.glMatrixMode(gl.GL_PROJECTION);				// Select The Projection Matrix
			gl.glLoadIdentity();							// Reset The Projection Matrix

			// Calculate The Aspect Ratio Of The Window
			// http://www.opengl.org/documentation/specs/man_pages/hardcopy/GL/html/glu/perspective.html
			glu.gluPerspective(45.0f,(float)width/(float)height,0.1f,100.0f);
			
			gl.glMatrixMode(gl.GL_MODELVIEW);				// Select The Modelview Matrix
			gl.glLoadIdentity();							// Reset The Modelview Matrix
		}
		
		// Drawing of pretty objects happens here
		protected void OnExposed (object o, EventArgs e)
		{
			if (this.MakeCurrent() == 0)
			  return;
			
			// Clear the scene
			gl.glClear (gl.GL_COLOR_BUFFER_BIT | gl.GL_DEPTH_BUFFER_BIT);
			
			// Replace current matrix with the identity matrix
			gl.glLoadIdentity ();
			gl.glClearColor (0.0f, 0.0f, 0.0f, 0.0f);

			// Translate a bit...
			gl.glLoadIdentity();
			gl.glTranslatef(0.0f,0.0f,-3.0f);				// Move away from the drawing area 3.0
			
			// Draw the GLObjects associated with this GLArea
			System.Collections.IEnumerator enumerator = GLObjectList.GetEnumerator();
  	
  			while(enumerator.MoveNext()){
  				( (GtkGL.IGLObject) enumerator.Current ).Draw();
  			}
					
			// bring back buffer to front, put front buffer in back
			this.SwapBuffers ();
		}

		// One-time configuration of opengl states happens here
		void OnRealized (object o, EventArgs e)
		{
			if (this.MakeCurrent() == 0)
			  return;

			gl.glClearColor (0.0f, 0.0f, 0.0f, 0.0f);
			gl.glClearDepth (1.0f);

			float[] materialSpecular = {1.0f, 1.0f, 1.0f, 0.15f};
			float[] materialShininess = {100.0f};
			float[] position = {-2.0f, 2.0f, 2.0f, 0.5f};

			Gl.glMaterialfv(Gl.GL_FRONT, Gl.GL_SPECULAR, materialSpecular);
			Gl.glMaterialfv(Gl.GL_FRONT, Gl.GL_SHININESS, materialShininess);
			Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_POSITION, position);

			Gl.glFrontFace (Gl.GL_CW);

			Gl.glEnable (Gl.GL_LIGHTING);
			Gl.glEnable (Gl.GL_LIGHT0);
			Gl.glEnable (Gl.GL_AUTO_NORMAL);
			Gl.glEnable (Gl.GL_NORMALIZE);
			Gl.glEnable (Gl.GL_DEPTH_TEST);
			Gl.glDepthFunc (Gl.GL_LEQUAL);
			gl.glShadeModel (gl.GL_SMOOTH);
			// Really Nice Perspective Calculations
			gl.glHint(gl.GL_PERSPECTIVE_CORRECTION_HINT, gl.GL_NICEST);
			
			// Iterate over associated IGLObject objects, calling Init() on each
			System.Collections.IEnumerator enumerator = GLObjectList.GetEnumerator();
  	
  			while(enumerator.MoveNext()){
  				( (GtkGL.IGLObject) enumerator.Current ).Init();
  			}
		}

		// This handler gets fired when the glArea widget is re-sized
		// This method is called "ReSizeGLScene" in the NeHe lessons
		void OnConfigure (object o, EventArgs e)
		{	
			if( this.MakeCurrent() == 0)
				return;
				
			gl.glViewport (0, 0, this.Allocation.Width, this.Allocation.Height);			
		}
		
		// Bound in glwidget.glade		
		private void OnQuit (object o, System.EventArgs e){
			Application.Quit();
		}

		// Bound in glwidget.glade
		private void OnWindowDeleteEvent (object sender, DeleteEventArgs a) 
		{
			Application.Quit ();
			a.RetVal = true;
		}

	}
}