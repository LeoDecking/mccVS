using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Operations;
using ProjectType.Resources;

namespace ProjectType.Intellisense
{
    [Export(typeof(ICompletionSourceProvider))]
    [ContentType("mcc")]
    [Name("statementCompletion")]
    internal class StatementCompletionSourceProvider : ICompletionSourceProvider
    {
        [Import]
        internal ITextStructureNavigatorSelectorService NavigatorService { get; set; }

        public ICompletionSource TryCreateCompletionSource(ITextBuffer textBuffer) => new StatementCompletionSource(textBuffer);
    }
    internal class StatementCompletionSource : ICompletionSource
    {
        private ITextBuffer textBuffer;
        private List<Completion> _completions;

        public StatementCompletionSource(ITextBuffer textBuffer)
        {
            this.textBuffer = textBuffer;
        }
        
        public void AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets)
        {
            _completions = Parser.GetCompletions(session.GetTriggerPoint(textBuffer).GetPoint(textBuffer.CurrentSnapshot));
            completionSets.Add(new CompletionSet("Tokens", "Tokens", FindTokenSpanAtPosition(session.GetTriggerPoint(textBuffer)),_completions, null));
        }

        private ITrackingSpan FindTokenSpanAtPosition(ITrackingPoint point)
        {
            int end = point.GetPosition(point.TextBuffer.CurrentSnapshot);
            int start = end - point.TextBuffer.CurrentSnapshot.GetText(0, end).Split(' ', '\n').Last().Length;

            Span span = Span.FromBounds(start, end);
            return point.TextBuffer.CurrentSnapshot.CreateTrackingSpan(span, SpanTrackingMode.EdgeInclusive);
        }

        private bool _isDisposed;
        public void Dispose()
        {
            if(!_isDisposed)
            {
                GC.SuppressFinalize(this);
                _isDisposed = true;
            }
        }
    }
}