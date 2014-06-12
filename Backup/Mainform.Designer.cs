namespace GL3DLab
{
  partial class Mainform
  {
    /// <summary>
    /// Erforderliche Designervariable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Verwendete Ressourcen bereinigen.
    /// </summary>
    /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
      thrOpenGL.Abort();
    }

    #region Vom Windows Form-Designer generierter Code

    /// <summary>
    /// Erforderliche Methode für die Designerunterstützung.
    /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
    /// </summary>
    private void InitializeComponent()
    {
      this.components = new System.ComponentModel.Container();
      this.view = new Tao.Platform.Windows.SimpleOpenGlControl();
      this.RepaintTimer = new System.Windows.Forms.Timer(this.components);
      this.SuspendLayout();
      // 
      // view
      // 
      this.view.AccumBits = ((byte)(0));
      this.view.AutoCheckErrors = false;
      this.view.AutoFinish = false;
      this.view.AutoMakeCurrent = true;
      this.view.AutoSize = true;
      this.view.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      this.view.AutoSwapBuffers = true;
      this.view.BackColor = System.Drawing.Color.Black;
      this.view.ColorBits = ((byte)(32));
      this.view.DepthBits = ((byte)(16));
      this.view.Dock = System.Windows.Forms.DockStyle.Fill;
      this.view.Location = new System.Drawing.Point(0, 0);
      this.view.Name = "view";
      this.view.Size = new System.Drawing.Size(585, 592);
      this.view.StencilBits = ((byte)(0));
      this.view.TabIndex = 0;
      this.view.Paint += new System.Windows.Forms.PaintEventHandler(this.OnPaintCanvas);
      this.view.KeyUp += new System.Windows.Forms.KeyEventHandler(this.view_KeyUp);
      this.view.Resize += new System.EventHandler(this.OnResizeCanvas);
      this.view.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnKeyDownCanvas);
      // 
      // RepaintTimer
      // 
      this.RepaintTimer.Enabled = true;
      this.RepaintTimer.Interval = 5;
      this.RepaintTimer.Tick += new System.EventHandler(this.RepaintTimer_Tick);
      // 
      // Mainform
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(585, 592);
      this.Controls.Add(this.view);
      this.Name = "Mainform";
      this.Text = "Form1";
      this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Mainform_FormClosing);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private Tao.Platform.Windows.SimpleOpenGlControl view;
    private System.Windows.Forms.Timer RepaintTimer;
  }
}

