/*
 * Copyright 2010-2015 Bastian Eicher
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

using System.IO;
using System.Linq;
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Storage;
using Org.BouncyCastle.Bcpg.OpenPgp;

namespace ZeroInstall.Store.Trust
{
    partial class BouncyCastle
    {
        private readonly string _publicBundlePath = Path.Combine(Locations.GetCacheDirPath("0install.net", machineWide: false), "pubring.gpg");

        [CanBeNull]
        private PgpPublicKeyRingBundle _publicBundle;

        /// <summary>
        /// Stores imported public keys on disk.
        /// Intentionally separate from the normal GnuPG public keyring to keep the user's GnuPG profile clean.
        /// </summary>
        /// <remarks>Data is cached in memory for life-time of instance.</remarks>
        [NotNull]
        private PgpPublicKeyRingBundle PublicBundle
        {
            get
            {
                // Multiple-read races are OK
                if (_publicBundle != null) return _publicBundle;

                try
                {
                    using (var stream = File.OpenRead(_publicBundlePath))
                        return _publicBundle = new PgpPublicKeyRingBundle(PgpUtilities.GetDecoderStream(stream));
                }
                    #region Error handling
                catch (DirectoryNotFoundException)
                {
                    return new PgpPublicKeyRingBundle(Enumerable.Empty<PgpPublicKeyRing>());
                }
                catch (FileNotFoundException)
                {
                    return new PgpPublicKeyRingBundle(Enumerable.Empty<PgpPublicKeyRing>());
                }
                catch (IOException ex)
                {
                    Log.Warn(ex);
                    return new PgpPublicKeyRingBundle(Enumerable.Empty<PgpPublicKeyRing>());
                }
                #endregion
            }
            set
            {
                // Lost-write races are OK, since public keys are easily reaquired
                _publicBundle = value;
                using (var atomic = new AtomicWrite(_publicBundlePath))
                {
                    using (var stream = File.Create(atomic.WritePath))
                        value.Encode(stream);
                    atomic.Commit();
                }
            }
        }

        [CanBeNull]
        private PgpSecretKeyRingBundle _secretBundle;

        /// <summary>
        /// Stores secret keys on disk.
        /// </summary>
        /// <remarks>Data is cached in memory for life-time of instance.</remarks>
        [NotNull]
        private PgpSecretKeyRingBundle SecretBundle
        {
            get
            {
                // Multiple-read races are OK
                if (_secretBundle != null) return _secretBundle;

                try
                {
                    using (var stream = File.OpenRead(Path.Combine(HomeDir, "secring.gpg")))
                        return _secretBundle = new PgpSecretKeyRingBundle(PgpUtilities.GetDecoderStream(stream));
                }
                    #region Error handling
                catch (DirectoryNotFoundException)
                {
                    return new PgpSecretKeyRingBundle(Enumerable.Empty<PgpSecretKeyRing>());
                }
                catch (FileNotFoundException)
                {
                    return new PgpSecretKeyRingBundle(Enumerable.Empty<PgpSecretKeyRing>());
                }
                catch (IOException ex)
                {
                    Log.Warn(ex);
                    return new PgpSecretKeyRingBundle(Enumerable.Empty<PgpSecretKeyRing>());
                }
                #endregion
            }
        }
    }
}
