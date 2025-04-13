using System.ComponentModel;

namespace Asteroids.WinForms
{
    public sealed partial class FrmAsteroids
    {
        #region Setup

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private readonly Container _components = null;

        protected override void Dispose(bool disposing)
        {
            _controller.SoundPlayed -= OnSoundPlayed;

            if (disposing)
                _components?.Dispose();


            foreach (var player in _soundPlayers)
                player.Value.Dispose();

            _controller.Dispose();

            base.Dispose(disposing);
        }

        #endregion

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            _frame1 = new Asteroids.WinForms.Classes.GraphicPictureBox();
            ((ISupportInitialize)_frame1).BeginInit();
            SuspendLayout();
            // 
            // _frame1
            // 
            _frame1.BackColor = System.Drawing.SystemColors.WindowText;
            _frame1.Dock = System.Windows.Forms.DockStyle.Fill;
            _frame1.Location = new System.Drawing.Point(0, 0);
            _frame1.Name = "_frame1";
            _frame1.Size = new System.Drawing.Size(634, 461);
            _frame1.TabIndex = 2;
            _frame1.TabStop = false;
            // 
            // FrmAsteroids
            // 
            AutoScaleBaseSize = new System.Drawing.Size(6, 16);
            ClientSize = new System.Drawing.Size(634, 461);
            Controls.Add(_frame1);
            Name = "FrmAsteroids";
            Text = "Asteroids";
            WindowState = System.Windows.Forms.FormWindowState.Maximized;
            Activated += frmAsteroids_Activated;
            Closed += frmAsteroids_Closed;
            KeyDown += frmAsteroids_KeyDown;
            KeyUp += frmAsteroids_KeyUp;
            Resize += frmAsteroids_Resize;
            ((ISupportInitialize)_frame1).EndInit();
            ResumeLayout(false);

        }

        #endregion

    }
}