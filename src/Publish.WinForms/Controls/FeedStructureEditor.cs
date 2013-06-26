/*
 * Copyright 2011-2013 Bastian Eicher, Simon E. Silva Lauinger
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using Common;
using Common.StructureEditor;
using ZeroInstall.Model;
using ZeroInstall.Model.Capabilities;

namespace ZeroInstall.Publish.WinForms.Controls
{
    public class FeedStructureEditor : StructureEditorControl<Feed>
    {
        public FeedStructureEditor()
        {
            DescribeRoot<SummaryEditor<Feed>>("interface");

            Describe<Feed>()
                .AddPlainList("icon", x => x.Icons)
                .AddPlainList("feed", x => x.Feeds)
                .AddPlainList("feed-for", x => x.FeedFor)
                .AddProperty("replaced-by", x => new PropertyPointer<InterfaceReference>(() => x.ReplacedBy, value => x.ReplacedBy = value))
                .AddPlainList<EntryPoint, SummaryEditor<EntryPoint>>("entry-point", x => x.EntryPoints)
                .AddPlainList("capabilities", x => x.CapabilityLists);

            Describe<EntryPoint>()
                .AddPlainList("icon", x => x.Icons);

            Describe<IElementContainer>()
                .AddList(x => x.Elements)
                .AddElement<Implementation>("implementation")
                .AddElement<PackageImplementation>("package-implementation")
                .AddElement<Group>("group");

            Describe<IDependencyContainer>()
                .AddPlainList("dependency", x => x.Dependencies)
                .AddPlainList("restriction", x => x.Restrictions);

            Describe<Restriction>()
                .AddPlainList("version", x => x.Constraints);

            Describe<IBindingContainer>()
                .AddList(x => x.Bindings)
                .AddElement<GenericBinding>("binding")
                .AddElement<EnvironmentBinding>("environment-binding")
                .AddElement<OverlayBinding>("overlay-binding")
                .AddElement<ExecutableInVar>("executable-in-var")
                .AddElement<ExecutableInPath>("executable-in-path");

            Describe<Command>()
                .AddProperty("runner", x => new PropertyPointer<Runner>(() => x.Runner, value => x.Runner = value))
                .AddProperty("working-dir", x => new PropertyPointer<WorkingDir>(() => x.WorkingDir, value => x.WorkingDir = value));

            Describe<IArgBaseContainer>()
                .AddList(x => x.Arguments)
                .AddElement<Arg>("arg")
                .AddElement<ForEachArgs>("for-each");
            Describe<ForEachArgs>().AddPlainList("arg", x => x.Arguments);

            Describe<Element>()
                .AddPlainList("command", x => x.Commands);

            Describe<Implementation>()
                .AddList(implementation => implementation.RetrievalMethods)
                .AddElement<Archive>("archive")
                .AddElement<Recipe>("recipe");

            Describe<Recipe>()
                .AddList(x => x.Steps)
                .AddElement<Archive>("archive")
                .AddElement<SingleFile>("single-file")
                .AddElement<RenameStep>("rename")
                .AddElement<RemoveStep>("remove");

            Describe<CapabilityList>()
                .AddList(x => x.Entries)
                .AddElement<AppRegistration>("registration")
                .AddElement<AutoPlay, DescriptionEditor<AutoPlay>>("auto-play")
                .AddElement<ComServer>("com-server")
                .AddElement<ContextMenu>("context-menu")
                .AddElement<DefaultProgram, DescriptionEditor<DefaultProgram>>("default-program")
                .AddElement<FileType, DescriptionEditor<FileType>>("file-type")
                .AddElement<GamesExplorer>("games-explorer")
                .AddElement<UrlProtocol, DescriptionEditor<UrlProtocol>>("url-protocol");
            Describe<AutoPlay>().AddPlainList("event", x => x.Events);
            Describe<FileType>().AddPlainList("extension", x => x.Extensions);
            Describe<UrlProtocol>().AddPlainList("known-prefix", x => x.KnownPrefixes);
            Describe<IconCapability>().AddPlainList("icon", x => x.Icons);
            Describe<VerbCapability>().AddPlainList<Verb, DescriptionEditor<Verb>>("verb", x => x.Verbs);
        }

        //        var digestProvider = editor as IDigestProvider;
        //        var implementation = parent as ImplementationBase;
        //        if (digestProvider != null && implementation != null)
        //        {
        //            // ToDo: Warn when changing an existing digest
        //
        //            // Set the ManifestDigest entry
        //            commandList.Add(new SetValueCommand<ManifestDigest>(
        //                new PropertyPointer<ManifestDigest>(() => implementation.ManifestDigest, newValue => implementation.ManifestDigest = newValue),
        //                digestProvider.ManifestDigest));
        //
        //            // Set the implementation ID unless its already something custom
        //            if (string.IsNullOrEmpty(implementation.ID) || implementation.ID.StartsWith("sha1=new"))
        //            {
        //                commandList.Add(new SetValueCommand<string>(
        //                    new PropertyPointer<string>(() => implementation.ID, newValue => implementation.ID = newValue),
        //                    "sha1new=" + digestProvider.ManifestDigest.Sha1New));
        //            }
        //        }
    }
}
