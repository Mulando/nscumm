namespace NScumm.Tmp
{
	[System.Diagnostics.DebuggerDisplay ("{DebuggerDisplay,nq}")]
    public class SimpleName: Expression
    {
        public string Name
        {
            get;
            private set;
        }

		internal override string DebuggerDisplay {
			get { return Name; }    
		}

        public SimpleName(string name)
        {
            this.Name = name;
        }

        #region implemented abstract members of Expression

        public override void Accept(IAstNodeVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override T Accept<T>(IAstNodeVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        #endregion
    }
}

