namespace RoadRegistry.Framework.Testing
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    public class ScenarioPrinter
    {
        public ScenarioPrinter(TextWriter writer, IMessageTranslator translator)
        {
            Writer = writer ?? throw new ArgumentNullException(nameof(writer));
            Translator = translator ?? throw new ArgumentNullException(nameof(translator));
        }

        public TextWriter Writer { get; }
        public IMessageTranslator Translator { get; }

        public async Task WriteAsync(ExpectEventsScenario scenario)
        {
            if (scenario == null)
            {
                throw new ArgumentNullException(nameof(scenario));
            }

            await Writer.WriteLineAsync("Given");
            await Writer.WriteLineAsync("");

            foreach (var given in scenario.Givens)
            {
                await Writer.WriteLineAsync(string.Format("\t[{0}] {1}", given.Stream, Translator.Translate(given.Event)));
            }

            await Writer.WriteLineAsync("When");
            await Writer.WriteLineAsync("");

            await Writer.WriteLineAsync(string.Format("\t{0}", Translator.Translate(scenario.When.Body)));
//            foreach (var header in scenario.When.Head)
//            {
//                await Writer.WriteLineAsync(string.Format("\t{0}", Translator.Translate(header)));
//            }

            await Writer.WriteLineAsync("Then");
            await Writer.WriteLineAsync("");

            foreach (var then in scenario.Thens)
            {
                await Writer.WriteLineAsync(string.Format("\t[{0}] {1}", then.Stream, Translator.Translate(then.Event)));
            }
        }

        public async Task WriteAsync(ExpectExceptionScenario scenario)
        {
            if (scenario == null)
            {
                throw new ArgumentNullException(nameof(scenario));
            }

            await Writer.WriteLineAsync("Given");
            await Writer.WriteLineAsync("");

            foreach (var given in scenario.Givens)
            {
                await Writer.WriteLineAsync(string.Format("\t[{0}] {1}", given.Stream, Translator.Translate(given.Event)));
            }

            await Writer.WriteLineAsync("When");
            await Writer.WriteLineAsync("");

            await Writer.WriteLineAsync(string.Format("\t{0}", Translator.Translate(scenario.When.Body)));
//            foreach (var header in scenario.When.Head)
//            {
//                await Writer.WriteLineAsync(string.Format("\t{0}", Translator.Translate(header)));
//            }

            await Writer.WriteLineAsync("Throws");
            await Writer.WriteLineAsync("");

            await Writer.WriteLineAsync(string.Format("\t{0}", Translator.Translate(scenario.Throws)));
        }
    }
}
