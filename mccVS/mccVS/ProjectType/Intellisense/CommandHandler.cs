using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;
using ProjectType.Resources;
using System;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Text;

namespace ProjectType.Intellisense
{
    [Export(typeof(IVsTextViewCreationListener))]
    [Name("commandHandler")]
    [ContentType("mcc")]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    internal class CommandHandlerProvider : IVsTextViewCreationListener
    {
        [Import]
        internal IVsEditorAdaptersFactoryService AdapterService;
        [Import]
        internal ICompletionBroker CompletionBroker { get; set; }
        [Import]
        internal SVsServiceProvider ServiceProvider { get; set; }
        [Import]
        internal ISignatureHelpBroker SignatureHelpBroker;
        public void VsTextViewCreated(IVsTextView textViewAdapter)

        {
            new Commands();

            ITextView textView = AdapterService.GetWpfTextView(textViewAdapter);
            if (textView == null)
                return;

            Func<CommandHandler> createCommandHandler = delegate { return new CommandHandler(textViewAdapter, textView, this, SignatureHelpBroker); };
            textView.Properties.GetOrCreateSingletonProperty(createCommandHandler);
        }
    }

    internal class CommandHandler : IOleCommandTarget
    {
        private IOleCommandTarget nextCommandHandler;
        private ITextView textView;
        private CommandHandlerProvider provider;
        private ICompletionSession _completionSession;
        private ISignatureHelpSession _signatureHelpSession;
        private ISignatureHelpBroker broker;

        internal CommandHandler(IVsTextView textViewAdapter, ITextView textView, CommandHandlerProvider provider, ISignatureHelpBroker broker)
        {
            this.textView = textView;
            this.provider = provider;
            this.broker = broker;

            textViewAdapter.AddCommandFilter(this, out nextCommandHandler);
        }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            return nextCommandHandler.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }

        public int Exec(ref Guid pguidCmdGroup, uint nCmdId, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (VsShellUtilities.IsInAutomationFunction(provider.ServiceProvider))
                return nextCommandHandler.Exec(ref pguidCmdGroup, nCmdId, nCmdexecopt, pvaIn, pvaOut);

            uint commandId = nCmdId;
            char typedChar = char.MinValue;
            if (pguidCmdGroup == VSConstants.VSStd2K && nCmdId == (uint)VSConstants.VSStd2KCmdID.TYPECHAR)
                typedChar = (char)(ushort)Marshal.GetObjectForNativeVariant(pvaIn);

            if (nCmdId == (ulong) VSConstants.VSStd2KCmdID.RETURN || char.IsWhiteSpace(typedChar))
            {
                if(_completionSession!= null && !_completionSession.IsDismissed)
                    if (_completionSession.SelectedCompletionSet.SelectionStatus.IsSelected)
                    {
                        _completionSession.Commit();
                        if(!char.IsWhiteSpace(typedChar))
                            return VSConstants.S_OK;
                    }
                    else
                    {
                        _completionSession.Dismiss();
                    }
                if (_signatureHelpSession != null)
                {
                    _signatureHelpSession.Dismiss();
                    _signatureHelpSession = null;
                }
                if (char.IsWhiteSpace(typedChar))
                    _signatureHelpSession = broker.TriggerSignatureHelp(textView);
            }
            int retVal = nextCommandHandler.Exec(ref pguidCmdGroup, nCmdId, nCmdexecopt, pvaIn, pvaOut);
            if (!typedChar.Equals(char.MinValue) &&
                (char.IsLetterOrDigit(typedChar) || typedChar == '.' || typedChar == '/' || char.IsWhiteSpace(typedChar)))
            {
                if (_completionSession == null || _completionSession.IsDismissed)
                    TriggerCompletion();
                _completionSession.Filter();
                return VSConstants.S_OK;
            }
            if (commandId == (ulong) VSConstants.VSStd2KCmdID.BACKSPACE || commandId == (ulong) VSConstants.VSStd2KCmdID.DELETE)
            {
                if (_completionSession != null && !_completionSession.IsDismissed)
                    _completionSession.Filter();
                return VSConstants.S_OK;
            }

            return retVal;
        }

        private void TriggerCompletion()
        {
            SnapshotPoint? caretPoint =
                textView.Caret.Position.Point.GetPoint(textBuffer => (!textBuffer.ContentType.IsOfType("projection")),
                    PositionAffinity.Predecessor);
            if (!caretPoint.HasValue)
                return;

            _completionSession = provider.CompletionBroker.CreateCompletionSession(textView,
                caretPoint.Value.Snapshot.CreateTrackingPoint(caretPoint.Value.Position, PointTrackingMode.Positive),
                    true);
            _completionSession.Dismissed += OnSessionDismissed;
            _completionSession.Start();
        }

        private void OnSessionDismissed(object sender, EventArgs e)
        {
            _completionSession.Dismissed -= OnSessionDismissed;
            _completionSession = null;
        }
    }
}
