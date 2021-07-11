using System.Collections.Generic;
using DigBuild.Render.Models.Expressions;

namespace DigBuild.Render.Models.Geometry
{
    public interface IPartialGeometry
    {
        IEnumerable<string> RequiredVariables { get; }

        IPartialGeometry ApplySubstitutions(IReadOnlyDictionary<string, IModelExpression> variables);

        IRawGeometry Prime();
    }
}