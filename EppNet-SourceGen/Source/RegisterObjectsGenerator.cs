/////////////////////////////////////////////
/// Filename: RegisterObjectsGenerator.cs
/// Date: January 6, 2025
/// Authors: Maverick Liberty
//////////////////////////////////////////////
using EppNet.SourceGen.Models;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;

using static EppNet.SourceGen.Globals;


namespace EppNet.SourceGen
{

    [Generator]
    public class RegisterObjectsGenerator : IIncrementalGenerator
    {

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {

            ConcurrentDictionary<string, string> resolverDict = new();

            // Let's locate our resolvers
            var resolvers = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                    NetTypeResolverAttrFullName,
                    predicate: static (node, _) => node is ClassDeclarationSyntax,
                    transform: static (ctx, ct) => TryCreateResolver(false, ctx.TargetNode as ClassDeclarationSyntax, ctx.SemanticModel, ct).Item1
                )
                .Where(static r => r is not null)
                .Collect()
                .Select(static (list, _) => list.ToDictionary(r => r!.Value.ResolvedTypeFullName, r => r!.Value.Name));

            var netObjects = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                    NetObjectAttrFullName,
                    predicate: static (node, _) => node is ClassDeclarationSyntax,
                    transform: (ctx, ct) => ((ctx.TargetNode as CSharpSyntaxNode), ctx.SemanticModel)
                )
                .Where(static t => t.Item1 is not null)
                .Collect()
                .Combine(resolvers)
                .Select(static ((ImmutableArray<(CSharpSyntaxNode, SemanticModel)>, Dictionary<string, string>) contents, CancellationToken ct) =>
                {
                    ImmutableArray<(CSharpSyntaxNode, SemanticModel)> array = contents.Item1;
                    Dictionary<string, NetworkObjectModel> objects = new();
                    ExecutionContext context = new(false);
                    context.Resolvers = contents.Item2;

                    foreach ((CSharpSyntaxNode node, SemanticModel semModel) in array)
                    {
                        NetworkObjectModel? model = TryCreateNetObject(context, node, semModel).Item1;

                        if (model.HasValue)
                            objects[model.Value.FullyQualifiedName] = model.Value;
                    }

                    return objects;
                });

            context.RegisterSourceOutput(netObjects.Combine(resolvers), static (SourceProductionContext spc, (Dictionary<string, NetworkObjectModel>, Dictionary<string, string>) b) =>
            {

                (Dictionary<string, NetworkObjectModel> objs, Dictionary<string, string> resolverDict) = b;

                foreach (KeyValuePair<string, NetworkObjectModel> kvp in objs)
                {
                    string fqn = kvp.Key;
                    NetworkObjectModel model = kvp.Value;

                    StringBuilder availableMethods = new();

                    Dictionary<string, EquatableHashSet<NetworkMethodModel>> fullMethods = new();
                    List<string> hierarchy = new(model.NetObjectHierarchy);
                    hierarchy.Add(fqn);

                    bool invalid = false;

                    foreach (string fullyQualifiedAncestorName in hierarchy)
                    {
                        if (!objs.TryGetValue(fullyQualifiedAncestorName, out NetworkObjectModel ancestor))
                        {
                            invalid = true;
                            break;
                        }
                        
                        foreach (KeyValuePair<string, EquatableHashSet<NetworkMethodModel>> ancKvp in ancestor.Methods)
                        {
                            string methodName = ancKvp.Key;

                            if (!fullMethods.TryGetValue(methodName, out EquatableHashSet<NetworkMethodModel> myMethods))
                                fullMethods[methodName] = ancKvp.Value;
                            else
                                myMethods.UnionWith(ancKvp.Value);
                        }
                    }

                    if (invalid)
                        continue;

                    foreach (KeyValuePair<string, EquatableHashSet<NetworkMethodModel>> kvp2 in fullMethods)
                    {
                        EquatableHashSet<NetworkMethodModel> methods = kvp2.Value;

                        foreach (NetworkMethodModel method in methods)
                            availableMethods.AppendLine($"// - {method}");
                    }

                    if (availableMethods.Length == 0)
                        availableMethods.Append($"// no methods found?!?! HELLO!? Methods.Count: {fullMethods.Count}");

                    StringBuilder hierarchyBuilder = new();

                    if (model.NetObjectHierarchy == null)
                        hierarchyBuilder.AppendLine("// - System.Object");
                    else
                    {
                        for (int i = 0; i < hierarchy.Count - 1; i++)
                            hierarchyBuilder.AppendLine($"// - {hierarchy[i]}");
                    }

                    StringBuilder resolverBuilder = new();
                    foreach (string resolverType in resolverDict.Keys)
                        resolverBuilder.AppendLine($"// {resolverType}");

                    StringBuilder builder = new($$"""
                        // <auto-generated/>
                        // full {{model.FullNamespace}}
                        // resolvers
                        {{resolverBuilder}}
                        // hierarchy:
                        {{hierarchyBuilder.ToString()}}
                        // available methods:
                        {{availableMethods.ToString()}}
                        // dist: {{model.Distribution}}

                        using EppNet.Logging;
                        using EppNet.Node;
                        using EppNet.Objects;
                        using EppNet.Utilities;

                        using System.Diagnostics.CodeAnalysis;

                        namespace {{model.FullNamespace}}
                        {

                            public partial class {{model.Name}} : {{NetworkObjectInternalInterfaceName}}
                            {
                    
                                public ILoggable Notify { get => this; }

                                public NetworkNode Node { get; }
                                public ObjectService Service { get; }
                        
                                public {{model.Name}}([NotNull] ObjectService service)
                                {
                                    Guard.AgainstNull(service);
                                    Node = service.Node;
                                    Service = service;
                                }
                    
                            }

                        }



                        """);

                    string filename = $"{model.Name}.g.cs";
                    spc.AddSource(filename, builder.ToString());
                }

            });

        }


    }


}
