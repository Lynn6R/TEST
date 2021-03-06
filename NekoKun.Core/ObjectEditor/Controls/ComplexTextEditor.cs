﻿using System;
using System.Collections.Generic;
using System.Text;
using NekoKun.Serialization.RubyMarshal;

namespace NekoKun.ObjectEditor
{
    public class ComplexTextEditor : AbstractObjectEditor
    {
        UI.Scintilla control;

        public ComplexTextEditor(Dictionary<string, object> Params)
            : base(Params)
        {
            control = new UI.Scintilla();
            control.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            control.ModifiedChanged += new EventHandler(control_ModifiedChanged);
        }

        void control_ModifiedChanged(object sender, EventArgs e)
        {
            MakeDirty();
        }

        public override void Commit()
        {
            this.selectedItem = new RubyString(control.Text);
        }

        protected override void InitControl()
        {
            control.Text = (selectedItem as RubyString).Text;
        }

        public override System.Windows.Forms.Control Control
        {
            get { return control; }
        }
    }
}
