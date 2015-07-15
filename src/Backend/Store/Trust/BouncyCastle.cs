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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Native;
using NanoByte.Common.Storage;
using NanoByte.Common.Streams;
using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Bcpg.OpenPgp;

namespace ZeroInstall.Store.Trust
{
    /// <summary>
    /// Provides access to the OpenPGP signature functions of Bouncy Castle.
    /// </summary>
    /// <remarks>This class is thread-safe.</remarks>
    public class BouncyCastle : IOpenPgp
    {
        /// <inheritdoc/>
        public IEnumerable<OpenPgpSignature> Verify(byte[] data, byte[] signature)
        {
            #region Sanity checks
            if (data == null) throw new ArgumentNullException("data");
            if (signature == null) throw new ArgumentNullException("signature");
            #endregion

            var signatureList = ParseObject<PgpSignatureList>(new MemoryStream(signature));

            var result = new OpenPgpSignature[signatureList.Count];
            for (int i = 0; i < result.Length; i++)
                result[i] = Verify(data, signatureList[i]);
            return result;
        }

        [NotNull]
        private OpenPgpSignature Verify([NotNull] byte[] data, [NotNull] PgpSignature signature)
        {
            var key = PublicBundle.GetPublicKey(signature.KeyId);

            if (key == null)
                return new MissingKeySignature(signature.KeyId);
            else
            {
                signature.InitVerify(key);
                signature.Update(data);

                if (signature.Verify())
                    return new ValidSignature(PublicBundle.GetPublicKey(signature.KeyId).GetFingerprint(), signature.CreationTime);
                else
                {
                    var badSig = new BadSignature(signature.KeyId);
                    Log.Warn(badSig.ToString());
                    return badSig;
                }
            }
        }

        /// <inheritdoc/>
        public byte[] Sign(byte[] data, OpenPgpSecretKey secretKey, string passphrase = null)
        {
            #region Sanity checks
            if (data == null) throw new ArgumentNullException("data");
            if (secretKey == null) throw new ArgumentNullException("secretKey");
            #endregion

            var pgpSecretKey = SecretBundle.GetSecretKey(secretKey.KeyIDParsed);
            if (pgpSecretKey == null) throw new KeyNotFoundException("Specified OpenPGP key not found on system");
            var pgpPrivateKey = GetPrivateKey(pgpSecretKey, passphrase);

            var signatureGenerator = new PgpSignatureGenerator(pgpSecretKey.PublicKey.Algorithm, HashAlgorithmTag.Sha1);
            signatureGenerator.InitSign(PgpSignature.BinaryDocument, pgpPrivateKey);
            signatureGenerator.Update(data);
            return signatureGenerator.Generate().GetEncoded();
        }

        [NotNull]
        private static PgpPrivateKey GetPrivateKey([NotNull] PgpSecretKey secretKey, [CanBeNull] string passphrase)
        {
            try
            {
                return secretKey.ExtractPrivateKey((passphrase == null) ? null : passphrase.ToCharArray());
            }
            catch (PgpException)
            {
                throw new WrongPassphraseException();
            }
        }

        /// <inheritdoc/>
        public void ImportKey(byte[] data)
        {
            #region Sanity checks
            if (data == null) throw new ArgumentNullException("data");
            #endregion

            var stream = PgpUtilities.GetDecoderStream(new MemoryStream(data));
            var ring = ParseObject<PgpPublicKeyRing>(stream);
            PublicBundle = PgpPublicKeyRingBundle.AddPublicKeyRing(PublicBundle, ring);
        }

        /// <inheritdoc/>
        public string ExportKey(OpenPgpSecretKey secretKey)
        {
            #region Sanity checks
            if (secretKey == null) throw new ArgumentNullException("secretKey");
            #endregion

            var key = PublicBundle.GetPublicKey(secretKey.KeyIDParsed);
            if (key == null) throw new KeyNotFoundException("Specified OpenPGP key not found on system");

            var output = new MemoryStream();
            using (var armored = new ArmoredOutputStream(output))
                key.Encode(armored);
            return output.ReadToString(Encoding.ASCII).Replace(Environment.NewLine, "\n");
        }

        /// <inheritdoc/>
        public IEnumerable<OpenPgpSecretKey> ListSecretKeys()
        {
            foreach (PgpSecretKeyRing ring in SecretBundle.GetKeyRings())
            {
                var key = ring.GetSecretKey();
                yield return new OpenPgpSecretKey(
                    key.KeyId,
                    key.PublicKey.GetFingerprint(),
                    key.UserIds.Cast<string>().First());
            }
        }

        [NotNull]
        private static T ParseObject<T>([NotNull] Stream stream) where T : PgpObject
        {
            try
            {
                return ParseObjects<T>(stream).First();
            }
                #region Error handling
            catch (InvalidOperationException)
            {
                throw new InvalidDataException("Unable to find instance of " + typeof(T).Name + " in stream");
            }
            catch (IOException ex)
            {
                throw new InvalidDataException(ex.Message, ex);
            }
            #endregion
        }

        [NotNull, ItemNotNull]
        private static IEnumerable<T> ParseObjects<T>([NotNull] Stream stream) where T : PgpObject
        {
            var factory = new PgpObjectFactory(stream);

            PgpObject obj;
            while ((obj = factory.NextPgpObject()) != null)
            {
                var target = obj as T;
                if (target != null) yield return target;

                var compressed = obj as PgpCompressedData;
                if (compressed != null)
                {
                    foreach (var result in ParseObjects<T>(compressed.GetDataStream()))
                        yield return result;
                }
            }
        }

        [CanBeNull]
        private PgpPublicKeyRingBundle _publicBundle;

        [NotNull]
        private PgpPublicKeyRingBundle PublicBundle
        {
            get
            {
                if (_publicBundle != null) return _publicBundle;

                try
                {
                    using (var stream = File.OpenRead(Path.Combine(HomeDir, "pubring.gpg")))
                        return _publicBundle = new PgpPublicKeyRingBundle(PgpUtilities.GetDecoderStream(stream));
                }
                    #region Error handling
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
            set { _publicBundle = value; }
        }

        private PgpSecretKeyRingBundle _secretBundle;

        [NotNull]
        private PgpSecretKeyRingBundle SecretBundle
        {
            get
            {
                if (_secretBundle != null) return _secretBundle;

                try
                {
                    using (var stream = File.OpenRead(Path.Combine(HomeDir, "secring.gpg")))
                        return _secretBundle = new PgpSecretKeyRingBundle(PgpUtilities.GetDecoderStream(stream));
                }
                    #region Error handling
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

        private static string HomeDir
        {
            get
            {
                return Locations.IsPortable
                    ? Locations.GetSaveConfigPath("0install.net", false, "gnupg")
                    : (WindowsUtils.IsWindows ? Locations.GetSaveConfigPath("gnupg", false) : Path.Combine(Locations.HomeDir, ".gnupg"));
            }
        }
    }
}
