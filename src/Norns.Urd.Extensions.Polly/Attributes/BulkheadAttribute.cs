using Norns.Urd.Reflection;
using Polly;

namespace Norns.Urd.Extensions.Polly
{
    public class BulkheadAttribute : AbstractPolicyAttribute
    {
        private readonly int maxParallelization;
        private readonly int maxQueuingActions;

        public BulkheadAttribute(int maxParallelization, int maxQueuingActions)
        {
            this.maxParallelization = maxParallelization;
            this.maxQueuingActions = maxQueuingActions;
        }

        public override ISyncPolicy Build(MethodReflector method)
        {
            return Policy.Bulkhead(maxParallelization, maxQueuingActions);
        }

        public override IAsyncPolicy BuildAsync(MethodReflector method)
        {
            return Policy.BulkheadAsync(maxParallelization, maxQueuingActions);
        }
    }
}