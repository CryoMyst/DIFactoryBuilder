using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIFactoryBuilder.SourceGenerator
{
    internal class IndentStringBuilderScope : IDisposable
    {
        private readonly IndentStringBuilder indentStringBuilder;

        public IndentStringBuilderScope(IndentStringBuilder indentStringBuilder)
        {
            this.indentStringBuilder = indentStringBuilder;
            this.indentStringBuilder.Indent();
        }

        public void Dispose()
        {
            this.indentStringBuilder.Outdent();
        }
    }
}
