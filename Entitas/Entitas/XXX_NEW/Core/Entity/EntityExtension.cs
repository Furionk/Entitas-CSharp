using Entitas.Serialization;

namespace Entitas {

    public static class EntityExtension {

        public const string COMPONENT_SUFFIX = "Component";

        public static string AddComponentSuffix(this string componentName) {
            return componentName.EndsWith(COMPONENT_SUFFIX, System.StringComparison.Ordinal)
                ? componentName
                : componentName + COMPONENT_SUFFIX;
        }

        public static string RemoveComponentSuffix(this string componentName) {
            return componentName.EndsWith(COMPONENT_SUFFIX, System.StringComparison.Ordinal)
                ? componentName.Substring(0, componentName.Length - COMPONENT_SUFFIX.Length)
                : componentName;
        }

        public static void CopyTo<TEntity>(this TEntity entity, TEntity target,
                                  bool replaceExisting = false, params int[] indices)
            where TEntity : class, IEntity, new() {
            var componentIndices = indices.Length == 0
                                        ? entity.GetComponentIndices()
                                        : indices;

            for(int i = 0; i < componentIndices.Length; i++) {
                var index = componentIndices[i];
                var component = entity.GetComponent(index);
                var clonedComponent = target.CreateComponent(
                    index, component.GetType()
                );
                component.CopyPublicMemberValues(clonedComponent);

                if(replaceExisting) {
                    target.ReplaceComponent(index, clonedComponent);
                } else {
                    target.AddComponent(index, clonedComponent);
                }
            }
        }
    }
}