/*
 * Copyright 2010-2014 Bastian Eicher
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

using System.ComponentModel;

namespace ZeroInstall.Store.Model.Design
{
    internal class LicenseNameConverter : StringConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return false;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(new[]
            {
                "",
                "AFL (Academic Free License)",
                "AFPL (Aladdin Free Public License)",
                "AGPL v1 (Affero General Public License)",
                "AGPL v2 (Affero General Public License)",
                "AGPL v3 (Affero General Public License)",
                "APL (Adaptive Public License)",
                "APSL v1 (Apple Public Source License)",
                "APSL v2 (Apple Public Source License)",
                "Artistic License",
                "BSD License (original)",
                "BSD License (revised)",
                "CDDL (Common Development and Distribution License)",
                "Common Public License",
                "Copyback License",
                "CVW (MITRE Collaborative Virtual Workspace License)",
                "DFSG approved (Debian Free Software Guidelines)",
                "EFL (Eiffel Forum License)",
                "EPL (Eclipse Public License)",
                "FDL (GNU Free Documentation License)",
                "Free for educational use",
                "Free for home use",
                "Free for non-commercial use",
                "Free to use but restricted",
                "Freely distributable",
                "Freeware",
                "GMGPL (GNAT Modified GPL)",
                "GPL v1 (GNU General Public License)",
                "GPL v2 (GNU General Public License)",
                "GPL v3 (GNU General Public License)",
                "Guile License",
                "IBM Public License",
                "LGPL (GNU Lesser General Public License)",
                "LPPL (The Latex Project Public License)",
                "MIT/X Consortium License",
                "MPL (Mozilla Public License)",
                "NOKOS (Nokia Open Source License)",
                "NPL (Netscape Public License)",
                "Open Software License",
                "OSI approved",
                "Other / Proprietary License",
                "Other / Proprietary License with Free Trial",
                "Other / Proprietary License with Source",
                "Perl License",
                "Public Domain",
                "Python License",
                "QPL (Q Public License)",
                "Ricoh Source Code Public License",
                "Shareware",
                "SUN Binary Code License",
                "SUN Community Source License",
                "SUN Public License",
                "The Apache License v1",
                "The Apache License v2",
                "The CeCILL License",
                "The Clarified Artistic License",
                "The Open Content License",
                "The PHP License",
                "VPL (Voxel Public License)",
                "W3C License",
                "WTFPL v1 (Do What The Fuck You Want To Public License)",
                "WTFPL v2 (Do What The Fuck You Want To Public License)",
                "zlib/libpng License",
                "ZPL (Zope Public License)"
            });
        }
    }
}
