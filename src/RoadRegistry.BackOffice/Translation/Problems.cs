namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using System.Collections.Immutable;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Model;

    public static class Problems
    {
        public static IFileProblemBuilder InFile(string file)
        {
            return new FileProblemBuilder(file);
        }

        private class FileProblemBuilder : IFileProblemBuilder, IFileDbaseRecordProblemBuilder, IFileShapeRecordProblemBuilder
        {
            private readonly string _file;
            private readonly ImmutableList<ProblemParameter> _parameters;

            public FileProblemBuilder(string file)
            {
                _file = file ?? throw new ArgumentNullException(nameof(file));
                _parameters = ImmutableList<ProblemParameter>.Empty;
            }

            private FileProblemBuilder(string file, ImmutableList<ProblemParameter> parameters)
            {
                _file = file;
                _parameters = parameters;
            }

            public IFileDbaseRecordProblemBuilder WithDbaseRecord(RecordNumber number)
            {
                return new FileProblemBuilder(_file, _parameters.Add(new ProblemParameter("DbaseRecordNumber", number.ToString())));
            }

            public IFileShapeRecordProblemBuilder WithShapeRecord(RecordNumber number)
            {
                return new FileProblemBuilder(_file, _parameters.Add(new ProblemParameter("ShapeRecordNumber", number.ToString())));
            }

            public IFileErrorBuilder Error(string reason)
            {
                if (reason == null) throw new ArgumentNullException(nameof(reason));

                return new FileErrorBuilder(_file, reason, _parameters);
            }

            public IFileWarningBuilder Warning(string reason)
            {
                if (reason == null) throw new ArgumentNullException(nameof(reason));

                return new FileWarningBuilder(_file, reason, _parameters);
            }

            private class FileErrorBuilder : IFileErrorBuilder
            {
                private readonly string _file;
                private readonly string _reason;
                private readonly ImmutableList<ProblemParameter> _parameters;

                public FileErrorBuilder(
                    string file,
                    string reason,
                    ImmutableList<ProblemParameter> parameters)
                {
                    _file = file;
                    _reason = reason;
                    _parameters = parameters;
                }

                public IFileErrorBuilder WithParameter(ProblemParameter parameter)
                {
                    if (parameter == null) throw new ArgumentNullException(nameof(parameter));

                    return new FileErrorBuilder(_file, _reason, _parameters.Add(parameter));
                }

                public IFileErrorBuilder WithParameters(params ProblemParameter[] parameters)
                {
                    if (parameters == null) throw new ArgumentNullException(nameof(parameters));

                    return new FileErrorBuilder(_file, _reason, _parameters.AddRange(parameters));
                }

                public FileError Build()
                {
                    return new FileError(_file.ToUpperInvariant(), _reason, _parameters.ToArray());
                }
            }

            private class FileWarningBuilder : IFileWarningBuilder
            {
                private readonly string _file;
                private readonly string _reason;
                private readonly ImmutableList<ProblemParameter> _parameters;

                public FileWarningBuilder(
                    string file,
                    string reason,
                    ImmutableList<ProblemParameter> parameters)
                {
                    _file = file;
                    _reason = reason;
                    _parameters = parameters;
                }

                public IFileWarningBuilder WithParameter(ProblemParameter parameter)
                {
                    if (parameter == null) throw new ArgumentNullException(nameof(parameter));

                    return new FileWarningBuilder(_file, _reason, _parameters.Add(parameter));
                }

                public IFileWarningBuilder WithParameters(params ProblemParameter[] parameters)
                {
                    if (parameters == null) throw new ArgumentNullException(nameof(parameters));

                    return new FileWarningBuilder(_file, _reason, _parameters.AddRange(parameters));
                }

                public FileWarning Build()
                {
                    return new FileWarning(_file.ToUpperInvariant(), _reason, _parameters.ToArray());
                }
            }
        }
    }
}