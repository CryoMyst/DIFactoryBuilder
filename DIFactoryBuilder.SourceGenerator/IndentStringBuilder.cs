using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIFactoryBuilder.SourceGenerator
{
    internal class IndentStringBuilder
    {
        private readonly StringBuilder stringBuilder = new StringBuilder();
        private readonly char indentChar = '\t';
        private readonly int charactersPerIndent = 1;


        public int CurrentIndent { get; private set; } = 0;


        public IndentStringBuilder(int startingIndent = 0)
        {
            this.CurrentIndent = startingIndent;
        }

        public IndentStringBuilder Append(string value)
        {
            this.stringBuilder.Append(new string(this.indentChar, this.CurrentIndent * this.charactersPerIndent))
                .Append(value);
            return this;
        }

        public IndentStringBuilder AppendNoIndent(string value)
        {
            this.stringBuilder.Append(value);
            return this;
        }


        public IndentStringBuilder AppendLine(string value)
        {
            this.stringBuilder.Append(new string(this.indentChar, this.CurrentIndent * this.charactersPerIndent))
                .AppendLine(value);
            return this;
        }

        public IndentStringBuilder AppendLine()
        {
            this.stringBuilder.AppendLine();
            return this;
        }

        public void Indent() => ++this.CurrentIndent;
        public void Outdent() => Math.Max(--this.CurrentIndent, 0);

        public IndentStringBuilderScope IndentScope => new IndentStringBuilderScope(this);

        public override string ToString()
        {
            return stringBuilder.ToString();
        }
    }
}
