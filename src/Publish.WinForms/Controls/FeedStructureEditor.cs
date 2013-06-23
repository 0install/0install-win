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
            Describe<Feed>()
                .AddPlainList(x => x.Icons)
                .AddPlainList(x => x.EntryPoints)
                .AddPlainList(x => x.CapabilityLists);

            Describe<EntryPoint>()
                .AddPlainList(x => x.Icons);

            Describe<IElementContainer>()
                .AddList(x => x.Elements)
                .AddElement<Implementation>()
                .AddElement<PackageImplementation>()
                .AddElement<Group>();

            Describe<IDependencyContainer>()
                .AddPlainList(x => x.Dependencies)
                .AddPlainList(x => x.Restrictions);

            Describe<Restriction>()
                .AddPlainList(x => x.Constraints);

            Describe<IBindingContainer>()
                .AddList(x => x.Bindings)
                .AddElement<EnvironmentBinding>()
                .AddElement<OverlayBinding>()
                .AddElement<ExecutableInVar>()
                .AddElement<ExecutableInPath>();

            Describe<Command>()
                .AddProperty(x => new PropertyPointer<Runner>(() => x.Runner, value => x.Runner = value))
                .AddProperty(x => new PropertyPointer<WorkingDir>(() => x.WorkingDir, value => x.WorkingDir = value));

            Describe<IArgBaseContainer>()
                .AddList(x => x.Arguments)
                .AddElement<Arg>()
                .AddElement<ForEachArgs>();
            Describe<ForEachArgs>().AddPlainList(x => x.Arguments);

            Describe<Element>()
                .AddPlainList(x => x.Commands);

            Describe<Implementation>()
                .AddList(implementation => implementation.RetrievalMethods)
                .AddElement<Archive>()
                .AddElement<Recipe>();

            Describe<Recipe>()
                .AddList(x => x.Steps)
                .AddElement<Archive>()
                .AddElement<SingleFile>()
                .AddElement<RenameStep>()
                .AddElement<RemoveStep>();

            Describe<CapabilityList>()
                .AddList(x => x.Entries)
                .AddElement<AppRegistration>()
                .AddElement<AutoPlay>()
                .AddElement<ComServer>()
                .AddElement<ContextMenu>()
                .AddElement<DefaultProgram>()
                .AddElement<FileType>()
                .AddElement<GamesExplorer>()
                .AddElement<UrlProtocol>();
            Describe<AutoPlay>().AddPlainList(x => x.Events);
            Describe<FileType>().AddPlainList(x => x.Extensions);
            Describe<UrlProtocol>().AddPlainList(x => x.KnownPrefixes);
            Describe<IconCapability>().AddPlainList(x => x.Icons);
            Describe<VerbCapability>().AddPlainList(x => x.Verbs);
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
