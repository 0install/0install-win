/*
 * Copyright 2011-2016 Bastian Eicher
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

using NanoByte.Common;
using NanoByte.Common.Storage;
using NanoByte.Common.StructureEditor;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Capabilities;

namespace ZeroInstall.Publish.WinForms.Controls
{
    /// <summary>
    /// A hierarchial <see cref="Feed"/> editor with Undo support.
    /// </summary>
    public class FeedStructureEditor : StructureEditorControl<Feed>
    {
        public FeedStructureEditor()
        {
            DescribeRoot<FeedEditor>("interface");

            Describe<IIconContainer>()
                .AddPlainList<Icon, IconEditor>("icon", x => x.Icons);

            Describe<Feed>()
                .AddPlainList("category", x => x.Categories)
                .AddPlainList("feed", x => x.Feeds)
                .AddPlainList("feed-for", x => x.FeedFor)
                .AddProperty("replaced-by", x => new PropertyPointer<InterfaceReference>(() => x.ReplacedBy, value => x.ReplacedBy = value))
                .AddPlainList<EntryPoint, EntryPointEditor>("entry-point", x => x.EntryPoints)
                .AddPlainList("capabilities", x => x.CapabilityLists);

            Describe<IDependencyContainer>()
                .AddPlainList("requires", x => x.Dependencies)
                .AddPlainList("restricts", x => x.Restrictions);

            Describe<IBindingContainer>()
                .AddList(x => x.Bindings)
                .AddElement<GenericBinding>("binding")
                .AddElement<EnvironmentBinding>("environment")
                .AddElement<OverlayBinding>("overlay")
                .AddElement<ExecutableInVar>("executable-in-var")
                .AddElement<ExecutableInPath>("executable-in-path");

            Describe<Element>()
                .AddPlainList("command", x => x.Commands);

            Describe<Implementation>()
                .AddList(implementation => implementation.RetrievalMethods)
                .AddElementContainerRef<Archive, ArchiveEditor>("archive")
                .AddElementContainerRef<SingleFile, SingleFileEditor>("file")
                .AddElementContainerRef<Recipe, RecipeEditor>("recipe");

            Describe<IElementContainer>()
                .AddList(x => x.Elements)
                .AddElement<Implementation>("implementation")
                .AddElement<PackageImplementation>("package-implementation")
                .AddElement<Group>("group");

            Describe<Restriction>()
                .AddPlainList("version", x => x.Constraints);

            Describe<Command>()
                .AddProperty("runner", x => new PropertyPointer<Runner>(() => x.Runner, value => x.Runner = value))
                .AddProperty("working-dir", x => new PropertyPointer<WorkingDir>(() => x.WorkingDir, value => x.WorkingDir = value));

            Describe<IArgBaseContainer>()
                .AddList(x => x.Arguments)
                .AddElement<Arg>("arg")
                .AddElement<ForEachArgs>("for-each");
            Describe<ForEachArgs>().AddPlainList("arg", x => x.Arguments);

            Describe<Recipe>()
                .AddList(x => x.Steps)
                .AddElement<Archive, ArchiveEditor>("archive")
                .AddElement<SingleFile, SingleFileEditor>("file")
                .AddElement<RenameStep>("rename")
                .AddElement<RemoveStep>("remove")
                .AddElement<CopyFromStep>("copy-from");

            Describe<CapabilityList>()
                .AddList(x => x.Entries)
                .AddElement<AppRegistration>("registration")
                .AddElement<AutoPlay, DescriptionEditor<AutoPlay>>("auto-play")
                .AddElement<ComServer>("com-server")
                .AddElement<ContextMenu>("context-menu")
                .AddElement<DefaultProgram, DescriptionEditor<DefaultProgram>>("default-program")
                .AddElement<FileType, DescriptionEditor<FileType>>("file-type")
                .AddElement<UrlProtocol, DescriptionEditor<UrlProtocol>>("url-protocol");
            Describe<AutoPlay>()
                .AddPlainList("event", x => x.Events);
            Describe<ISingleVerb>()
                .AddProperty<Verb, DescriptionEditor<Verb>>("verb", x => new PropertyPointer<Verb>(() => x.Verb, value => x.Verb = value));
            Describe<FileType>().AddPlainList("extension", x => x.Extensions);
            Describe<UrlProtocol>().AddPlainList("known-prefix", x => x.KnownPrefixes);
            Describe<VerbCapability>().AddPlainList<Verb, DescriptionEditor<Verb>>("verb", x => x.Verbs);
        }

        /// <inheritdoc/>
        protected override string ToXmlString()
        {
            return base.ToXmlString().
                // Hide XSI information
                Replace(" xmlns:xsi=\"" + XmlStorage.XsiNamespace + "\" xsi:schemaLocation=\"" + Feed.XsiSchemaLocation + "\"", "");
        }
    }
}
