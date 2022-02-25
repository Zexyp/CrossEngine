﻿namespace DemoFramework.DebugInfo
{
    partial class DebugInfoForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.worldTree = new System.Windows.Forms.TreeView();
            this.snapshotButton = new System.Windows.Forms.Button();
            this.debugDrawFlags = new System.Windows.Forms.CheckedListBox();
            this.SuspendLayout();
            // 
            // worldTree
            // 
            this.worldTree.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.worldTree.Location = new System.Drawing.Point(13, 42);
            this.worldTree.Name = "worldTree";
            this.worldTree.Size = new System.Drawing.Size(348, 333);
            this.worldTree.TabIndex = 0;
            // 
            // snapshotButton
            // 
            this.snapshotButton.Location = new System.Drawing.Point(13, 13);
            this.snapshotButton.Name = "snapshotButton";
            this.snapshotButton.Size = new System.Drawing.Size(108, 23);
            this.snapshotButton.TabIndex = 1;
            this.snapshotButton.Text = "Take snapshot";
            this.snapshotButton.UseVisualStyleBackColor = true;
            this.snapshotButton.Click += new System.EventHandler(this.snapshotButton_Click);
            // 
            // debugDrawFlags
            // 
            this.debugDrawFlags.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.debugDrawFlags.FormattingEnabled = true;
            this.debugDrawFlags.IntegralHeight = false;
            this.debugDrawFlags.Items.AddRange(new object[] {
            "Wireframe",
            "AABB",
            "Contact points",
            "Constraints",
            "Constraint limits",
            "Fast wireframe",
            "Normals",
            "Frames"});
            this.debugDrawFlags.Location = new System.Drawing.Point(367, 42);
            this.debugDrawFlags.Name = "debugDrawFlags";
            this.debugDrawFlags.Size = new System.Drawing.Size(246, 125);
            this.debugDrawFlags.TabIndex = 2;
            // 
            // DebugInfoForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(625, 470);
            this.Controls.Add(this.debugDrawFlags);
            this.Controls.Add(this.snapshotButton);
            this.Controls.Add(this.worldTree);
            this.Name = "DebugInfoForm";
            this.Text = "Debug Info";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView worldTree;
        private System.Windows.Forms.Button snapshotButton;
        private System.Windows.Forms.CheckedListBox debugDrawFlags;
    }
}