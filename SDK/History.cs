using System.Collections.Concurrent;
using QuikGraph;

namespace Agience.SDK
{
    public class InformationVertex
    {
        public string? Id { get; internal set; }
        public Data? Input { get; internal set; }
        public DateTime? InputTimestamp { get; internal set; }
        public Data? Output { get; internal set; }
        public DateTime? OutputTimestamp { get; internal set; }
        public string? FunctionId { get; internal set; }
    }

    public class InformationEdge : Edge<InformationVertex>
    {
        internal InformationEdge(InformationVertex source, InformationVertex target) : base(source, target) { }
    }

    public class History : BidirectionalGraph<InformationVertex, InformationEdge>
    {
        public string? Id { get; internal set; }
        public string? OwnerId { get; internal set; }

        private readonly ConcurrentDictionary<string, object> _vertexLocks = new ConcurrentDictionary<string, object>();
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        public History(string? id = null, string? ownerId = null)
        {
            Id = id;
            OwnerId = ownerId;
        }

        internal void Add(Information information)
        {
            InformationVertex? currentVertex;

            var currentVertexLock = _vertexLocks.GetOrAdd(information.Id, _ => new object());

            lock (currentVertexLock)
            {
                _lock.EnterWriteLock();
                try
                {
                    // TODO: Use indexed dictionary for performance
                    currentVertex = Vertices.FirstOrDefault(v => v.Id == information.Id);

                    if (currentVertex == null)
                    {
                        currentVertex = new InformationVertex()
                        {
                            Id = information.Id,
                            Input = information.Input,
                            InputTimestamp = DateTime.TryParse(information.InputTimestamp!, out var inputTimestamp) ? inputTimestamp : null,
                            Output = information.Output,
                            OutputTimestamp = DateTime.TryParse(information.OutputTimestamp!, out var outputTimestamp) ? outputTimestamp : null,
                            FunctionId = information.FunctionId,
                        };
                        AddVertex(currentVertex);
                    }
                    else // Information builds in stages, but once the value is there it should be immutable. Safe to overwrite.
                    {
                        currentVertex.Input = information.Input;
                        currentVertex.InputTimestamp = DateTime.TryParse(information.InputTimestamp!, out var inputTimestamp) ? inputTimestamp : null;
                        currentVertex.Output = information.Output;
                        currentVertex.OutputTimestamp = DateTime.TryParse(information.OutputTimestamp!, out var outputTimestamp) ? outputTimestamp : null;
                        currentVertex.FunctionId = information.FunctionId;
                    }
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }

            if (string.IsNullOrEmpty(information.ParentId)) { return; }

            var parentVertexLock = _vertexLocks.GetOrAdd(information.ParentId, _ => new object());

            lock (parentVertexLock)
            {
                _lock.EnterWriteLock();
                try
                {
                    // TODO: Use indexed dictionary for performance
                    var parentVertex = Vertices.FirstOrDefault(v => v.Id == information.ParentId);

                    if (parentVertex == null)
                    {
                        parentVertex = new InformationVertex()
                        {
                            Id = information.ParentId
                        };
                        AddVertex(parentVertex);
                    }

                    AddEdge(new InformationEdge(parentVertex, currentVertex));

                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
        }
    }
}
