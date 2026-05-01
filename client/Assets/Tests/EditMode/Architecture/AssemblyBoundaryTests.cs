using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace Game.Tests.EditMode.Architecture
{
    /// <summary>
    /// ADR-0003 / story-001：程序集边界与 UnityEngine 隔离。
    /// 验证三层程序集模型的引用方向正确。
    /// </summary>
    public sealed class AssemblyBoundaryTests
    {
        private static Assembly GameplayAssembly =>
            AppDomain.CurrentDomain.GetAssemblies()
                .First(a => a.GetName().Name == "game.gameplay");

        [Test]
        public void GameplayAssembly_DoesNotReference_UnityEngine()
        {
            var refs = GameplayAssembly.GetReferencedAssemblies();
            var unityRefs = refs.Where(r =>
                r.Name.StartsWith("UnityEngine", StringComparison.Ordinal) ||
                r.Name.StartsWith("UnityEditor", StringComparison.Ordinal))
                .ToList();

            Assert.That(unityRefs, Is.Empty,
                $"game.gameplay must not reference UnityEngine. Found: {string.Join(", ", unityRefs.Select(r => r.Name))}");
        }

        [Test]
        public void GameplayAssembly_DoesNotReference_UnityUI()
        {
            var refs = GameplayAssembly.GetReferencedAssemblies();
            var uiRefs = refs.Where(r =>
                r.Name == "Unity.TextMeshPro" ||
                r.Name == "UnityEngine.UI" ||
                r.Name == "Unity.Addressables")
                .ToList();

            Assert.That(uiRefs, Is.Empty,
                $"game.gameplay must not reference Unity UI/asset packages. Found: {string.Join(", ", uiRefs.Select(r => r.Name))}");
        }

        [Test]
        public void GameplayAssembly_IsLoadedInEditor()
        {
            Assert.That(GameplayAssembly, Is.Not.Null);
            Assert.That(GameplayAssembly.GetTypes().Length, Is.GreaterThan(0),
                "game.gameplay should contain types");
        }

        [Test]
        public void GameplayAssembly_NoCombatRulesInLaunchNamespaces()
        {
            var gameplayTypes = GameplayAssembly.GetTypes();

            var launchLeakTypes = gameplayTypes.Where(t =>
                t.Namespace != null && (
                    t.Namespace.Contains("Launch", StringComparison.Ordinal) ||
                    t.Namespace.Contains("Procedure", StringComparison.Ordinal) ||
                    t.Namespace.Contains("HybridCLR", StringComparison.Ordinal)))
                .ToList();

            Assert.That(launchLeakTypes, Is.Empty,
                $"game.gameplay should not contain Launch/Procedure/HybridCLR namespaces. Found: {string.Join(", ", launchLeakTypes.Select(t => t.FullName))}");
        }
    }
}
