using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Game.Gameplay.Events;
using NUnit.Framework;

namespace Game.Tests.EditMode.Architecture
{
    /// <summary>
    /// ADR-0003 / story-002：跨边界 DTO 仅基元与稳定 ID。
    /// 反射扫描 game.gameplay 中实现 IBattleEvent 的类型，
    /// 验证所有公开字段/属性类型属于白名单。
    /// </summary>
    public sealed class DtoBoundaryTypeTests
    {
        /// <summary>
        /// 跨 gameplay/Unity 边界允许的字段类型白名单（ADR-0003）。
        /// primitives + string + enums + project pure-math types + SceneEventContext 等 gameplay 结构体。
        /// </summary>
        private static readonly HashSet<Type> AllowedLeafTypes = new HashSet<Type>
        {
            typeof(bool),
            typeof(byte), typeof(sbyte),
            typeof(short), typeof(ushort),
            typeof(int), typeof(uint),
            typeof(long), typeof(ulong),
            typeof(float), typeof(double),
            typeof(string),
            typeof(decimal),
        };

        private static Assembly GameplayAssembly =>
            AppDomain.CurrentDomain.GetAssemblies()
                .First(a => a.GetName().Name == "game.gameplay");

        private static bool IsAllowedType(Type type)
        {
            if (AllowedLeafTypes.Contains(type))
                return true;

            if (type.IsEnum)
                return true;

            if (type.Assembly == GameplayAssembly && type.IsValueType)
                return true;

            if (Nullable.GetUnderlyingType(type) is { } underlying)
                return IsAllowedType(underlying);

            return false;
        }

        [Test]
        public void AllBattleEventDtos_UseOnlyAllowedFieldTypes()
        {
            var dtoTypes = GameplayAssembly.GetTypes()
                .Where(t => typeof(IBattleEvent).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                .ToList();

            if (dtoTypes.Count == 0)
            {
                Assert.Pass("No IBattleEvent DTOs in game.gameplay yet — check will activate when first DTO is added.");
                return;
            }

            var violations = new List<string>();

            foreach (var dtoType in dtoTypes)
            {
                var props = dtoType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (var prop in props)
                {
                    if (!IsAllowedType(prop.PropertyType))
                    {
                        violations.Add($"{dtoType.Name}.{prop.Name} : {prop.PropertyType.FullName}");
                    }
                }

                var fields = dtoType.GetFields(BindingFlags.Public | BindingFlags.Instance);
                foreach (var field in fields)
                {
                    if (!IsAllowedType(field.FieldType))
                    {
                        violations.Add($"{dtoType.Name}.{field.Name} : {field.FieldType.FullName}");
                    }
                }
            }

            Assert.That(violations, Is.Empty,
                $"DTO boundary type violations (only primitives/string/enum/gameplay structs allowed):\n  {string.Join("\n  ", violations)}");
        }

        [Test]
        public void AllBattleEventDtos_AreValueTypes()
        {
            var dtoTypes = GameplayAssembly.GetTypes()
                .Where(t => typeof(IBattleEvent).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                .ToList();

            var nonValueTypes = dtoTypes.Where(t => !t.IsValueType).Select(t => t.FullName).ToList();

            Assert.That(nonValueTypes, Is.Empty,
                $"Battle event DTOs should be value types (readonly struct). Non-value types: {string.Join(", ", nonValueTypes)}");
        }

        [Test]
        public void NoBattleEventDto_ReferencesUnityTypes()
        {
            var dtoTypes = GameplayAssembly.GetTypes()
                .Where(t => typeof(IBattleEvent).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                .ToList();

            var violations = new List<string>();
            foreach (var dtoType in dtoTypes)
            {
                var allMembers = dtoType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
                    .Select(p => (p.Name, TypeName: p.PropertyType.FullName ?? ""))
                    .Concat(dtoType.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
                        .Select(f => (f.Name, TypeName: f.FieldType.FullName ?? "")));

                foreach (var (name, typeName) in allMembers)
                {
                    if (typeName.StartsWith("UnityEngine", StringComparison.Ordinal) ||
                        typeName.StartsWith("UnityEditor", StringComparison.Ordinal))
                    {
                        violations.Add($"{dtoType.Name}.{name} : {typeName}");
                    }
                }
            }

            Assert.That(violations, Is.Empty,
                $"DTOs must not reference Unity types:\n  {string.Join("\n  ", violations)}");
        }
    }
}
