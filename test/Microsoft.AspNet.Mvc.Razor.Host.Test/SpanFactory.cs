﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.AspNet.Razor.Editor;
using Microsoft.AspNet.Razor.Generator;
using Microsoft.AspNet.Razor.Parser.SyntaxTree;
using Microsoft.AspNet.Razor.Text;
using Microsoft.AspNet.Razor.Tokenizer;
using Microsoft.AspNet.Razor.Tokenizer.Symbols;

namespace Microsoft.AspNet.Mvc.Razor.Host.Test
{
    public static class SpanFactoryExtensions
    {
        public static SpanConstructor EmptyHtml(this SpanFactory self)
        {
            return self.Span(SpanKind.Markup, new HtmlSymbol(self.LocationTracker.CurrentLocation, String.Empty, HtmlSymbolType.Unknown))
                .With(new MarkupCodeGenerator());
        }

        public static UnclassifiedCodeSpanConstructor Code(this SpanFactory self, string content)
        {
            return new UnclassifiedCodeSpanConstructor(
                self.Span(SpanKind.Code, content, markup: false));
        }

        public static SpanConstructor CodeTransition(this SpanFactory self, string content)
        {
            return self.Span(SpanKind.Transition, content, markup: false).Accepts(AcceptedCharacters.None);
        }

        public static SpanConstructor MetaCode(this SpanFactory self, string content)
        {
            return self.Span(SpanKind.MetaCode, content, markup: false);
        }
        public static SpanConstructor Markup(this SpanFactory self, string content)
        {
            return self.Span(SpanKind.Markup, content, markup: true).With(new MarkupCodeGenerator());
        }
    }

    public class SpanFactory
    {
        public Func<ITextDocument, ITokenizer> MarkupTokenizerFactory { get; set; }
        public Func<ITextDocument, ITokenizer> CodeTokenizerFactory { get; set; }
        public SourceLocationTracker LocationTracker { get; private set; }

        public static SpanFactory CreateCsHtml()
        {
            return new SpanFactory()
            {
                MarkupTokenizerFactory = doc => new HtmlTokenizer(doc),
                CodeTokenizerFactory = doc => new CSharpTokenizer(doc)
            };
        }

        public SpanFactory()
        {
            LocationTracker = new SourceLocationTracker();
        }


        public SpanConstructor Span(SpanKind kind, string content, bool markup)
        {
            return new SpanConstructor(kind, Tokenize(new[] { content }, markup));
        }

        public SpanConstructor Span(SpanKind kind, params ISymbol[] symbols)
        {
            return new SpanConstructor(kind, symbols);
        }

        private IEnumerable<ISymbol> Tokenize(IEnumerable<string> contentFragments, bool markup)
        {
            return contentFragments.SelectMany(fragment => Tokenize(fragment, markup));
        }

        private IEnumerable<ISymbol> Tokenize(string content, bool markup)
        {
            ITokenizer tok = MakeTokenizer(markup, new SeekableTextReader(content));
            ISymbol sym;
            ISymbol last = null;
            while ((sym = tok.NextSymbol()) != null)
            {
                OffsetStart(sym, LocationTracker.CurrentLocation);
                last = sym;
                yield return sym;
            }
            LocationTracker.UpdateLocation(content);
        }

        private ITokenizer MakeTokenizer(bool markup, SeekableTextReader seekableTextReader)
        {
            if (markup)
            {
                return MarkupTokenizerFactory(seekableTextReader);
            }
            else
            {
                return CodeTokenizerFactory(seekableTextReader);
            }
        }

        private void OffsetStart(ISymbol sym, SourceLocation sourceLocation)
        {
            sym.OffsetStart(sourceLocation);
        }
    }

    public static class SpanConstructorExtensions
    {
        public static SpanConstructor Accepts(this SpanConstructor self, AcceptedCharacters accepted)
        {
            return self.With(eh => eh.AcceptedCharacters = accepted);
        }
    }

    public class UnclassifiedCodeSpanConstructor
    {
        SpanConstructor _self;

        public UnclassifiedCodeSpanConstructor(SpanConstructor self)
        {
            _self = self;
        }

        public SpanConstructor As(ISpanCodeGenerator codeGenerator)
        {
            return _self.With(codeGenerator);
        }
    }

    public class SpanConstructor
    {
        public SpanBuilder Builder { get; private set; }

        internal static IEnumerable<ISymbol> TestTokenizer(string str)
        {
            yield return new RawTextSymbol(SourceLocation.Zero, str);
        }

        public SpanConstructor(SpanKind kind, IEnumerable<ISymbol> symbols)
        {
            Builder = new SpanBuilder();
            Builder.Kind = kind;
            Builder.EditHandler = SpanEditHandler.CreateDefault(TestTokenizer);
            foreach (ISymbol sym in symbols)
            {
                Builder.Accept(sym);
            }
        }

        private Span Build()
        {
            return Builder.Build();
        }

        public SpanConstructor With(ISpanCodeGenerator generator)
        {
            Builder.CodeGenerator = generator;
            return this;
        }

        public SpanConstructor With(SpanEditHandler handler)
        {
            Builder.EditHandler = handler;
            return this;
        }

        public SpanConstructor With(Action<SpanEditHandler> handlerConfigurer)
        {
            handlerConfigurer(Builder.EditHandler);
            return this;
        }

        public static implicit operator Span(SpanConstructor self)
        {
            return self.Build();
        }
    }

    internal class RawTextSymbol : ISymbol
    {
        public SourceLocation Start { get; private set; }
        public string Content { get; private set; }

        public RawTextSymbol(SourceLocation start, string content)
        {
            if (content == null)
            {
                throw new ArgumentNullException("content");
            }

            Start = start;
            Content = content;
        }

        public override bool Equals(object obj)
        {
            RawTextSymbol other = obj as RawTextSymbol;
            return Equals(Start, other.Start) && Equals(Content, other.Content);
        }

        internal bool EquivalentTo(ISymbol sym)
        {
            return Equals(Start, sym.Start) && Equals(Content, sym.Content);
        }

        public override int GetHashCode()
        {
            return Start.GetHashCode();
        }

        public void OffsetStart(SourceLocation documentStart)
        {
            Start = documentStart + Start;
        }

        public void ChangeStart(SourceLocation newStart)
        {
            Start = newStart;
        }

        public override string ToString()
        {
            return String.Format(CultureInfo.InvariantCulture, "{0} RAW - [{1}]", Start, Content);
        }

        internal void CalculateStart(Span prev)
        {
            if (prev == null)
            {
                Start = SourceLocation.Zero;
            }
            else
            {
                Start = new SourceLocationTracker(prev.Start).UpdateLocation(prev.Content).CurrentLocation;
            }
        }
    }
}
