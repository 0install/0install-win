// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

namespace ZeroInstall;

public static class LocalizableStrings
{
    public static string MachineWide => new LocalizableStringCollection
    {
        {"en", "For &all users"},
        {"de", "Für &alle Benutzer"},
        {"es", "&Todos los usuarios"},
        {"fr", "&Tous les utilisateurs"},
        {"it", "Per &tutti gli utenti"},
        {"ja", "すべてのユーザー"},
        {"nl", "Voor &alle gebruikers"},
        {"pl", "&Wszyscy użytkownicy"},
        {"pt", "&Todos os utilizadores"},
        {"ru", "Для всех пользователей"},
        {"zh", "面向所有用户"}
    }.GetBestLanguage(CultureInfo.CurrentUICulture)!;

    public static string Continue => new LocalizableStringCollection
    {
        {"en", "&Install"},
        {"de", "&Installieren"},
        {"es", "&Instalar"},
        {"fr", "&Installer"},
        {"it", "&Installa"},
        {"ja", "インストール"},
        {"nl", "&Installeren"},
        {"pl", "Zainstaluj"},
        {"pt", "&Instalar"},
        {"ru", "Установить"},
        {"zh", "安装"}
    }.GetBestLanguage(CultureInfo.CurrentUICulture)!;

    public static string Cancel => new LocalizableStringCollection
    {
        {"en", "Cancel"},
        {"de", "Abbrechen"},
        {"es", "Cancelar"},
        {"fr", "Annuler"},
        {"it", "Annulla"},
        {"ja", "キャンセル"},
        {"nl", "Annuleren"},
        {"pl", "Anuluj"},
        {"pt", "Cancelar"},
        {"ru", "Отменить"},
        {"zh", "取消"}
    }.GetBestLanguage(CultureInfo.CurrentUICulture)!;

    public static string DestinationFolder => new LocalizableStringCollection
    {
        {"en", "Install {0} here:"},
        {"de", "{0} hier installieren:"},
        {"es", "Instalar {0} aquí:"},
        {"fr", "Installer {0} ici :"},
        {"it", "Installa {0} qui:"},
        {"ja", "インストール先"},
        {"nl", "{0} hier installeren:"},
        {"pl", "Zainstaluj {0} tutaj:"},
        {"pt", "Instalar {0} aqui:"},
        {"ru", "Папка для установки:"},
        {"zh", "安装位置："}
    }.GetBestLanguage(CultureInfo.CurrentUICulture)!;

    public static string Change => new LocalizableStringCollection
    {
        {"en", "&Change"},
        {"de", "Ä&ndern"},
        {"es", "&Cambiar"},
        {"fr", "&Modifier"},
        {"it", "Sfoglia"},
        {"ja", "変更"},
        {"nl", "Wijzigen"},
        {"pl", "Zmień"},
        {"pt", "Alterar"},
        {"ru", "Изменить"},
        {"zh", "更改"}
    }.GetBestLanguage(CultureInfo.CurrentUICulture)!;

    public static string ChoosePath => new LocalizableStringCollection
    {
        {"en", "Select where to install {0}:"},
        {"de", "Wählen Sie einen Speicherort für {0}:"},
        {"es", "Selecciona dónde quieres instalar {0}:"},
        {"fr", "Sélectionnez l’emplacement où {0} devrait être installé:"},
        {"it", "Seleziona dove installare {0}:"},
        {"ja", "{0}のインストール先を選んでください。"},
        {"nl", "Waar wilt u {0} installeren?"},
        {"pl", "Wybierz, gdzie chcesz zainstalować {0}:"},
        {"pt-BR", "Selecione o local onde deseja instalar o {0}:"},
        {"pt-PT", "Selecione onde quer instalar o {0}:"},
        {"ru", "Выберите папку для установки {0}:"},
        {"zh", "选择你想安装{0}的位置："}
    }.GetBestLanguage(CultureInfo.CurrentUICulture)!;

    public static string FolderNotNtfs => new LocalizableStringCollection
    {
        {"en", "'{0}' is not on an NTFS-formatted disk.\nPlease choose another installation location."},
        {"de", "'{0}' befindet sich nicht auf einem NTFS-formatierten Datenträger.\nBitte wählen Sie einen anderen Installationsort."},
        {"es", "'{0}' no está en un disco con formato NTFS.\nPor favor, elija otro lugar de instalación."},
        {"fr", "'{0}' ne se trouve pas sur un disque formaté en NTFS.\nVeuillez choisir un autre emplacement d'installation."},
        {"it", "'{0}' non si trova su un disco formattato con NTFS.\nScegliere un'altra posizione di installazione."},
        {"ja", "{0}はNTFSでフォーマットされたディスクにありません。\n他のインストール先を選択してください。"},
        {"nl", "'{0}' staat niet op een NTFS-geformatteerde schijf.\nKies een andere installatielocatie."},
        {"pl", "'{0}' nie znajduje się na dysku sformatowanym w systemie NTFS.\nProszę wybrać inną lokalizację instalacji."},
        {"pt-BR", "'{0}' não está localizado em uma unidade formatada em NTFS.\nPor favor, escolha um local de instalação diferente."},
        {"pt-PT", "'{0}' não está localizado numa unidade em formato NTFS.\nPor favor, escolha um local de instalação diferente."},
        {"ru", "'{0}' не находится на диске, отформатированном в NTFS.\nПожалуйста, выберите другое место установки."},
        {"zh", "{0}不在一个以NTFS格式化的驱动器上。\n请选择另一个安装地点。"}
    }.GetBestLanguage(CultureInfo.CurrentUICulture)!;

    public static string FolderNotSupported => new LocalizableStringCollection
    {
        {"en", "'{0}' is not a supported location for installing software.\nPlease choose another installation location."},
        {"de", "'{0}' ist kein unterstützter Ort um Software zu installieren.\nBitte wählen Sie einen anderen Installationsort."},
        {"es", "'{0}' no es una ubicación admitida para instalar software.\nPor favor, elija otro lugar de instalación."},
        {"fr", "'{0}' n'est pas un emplacement pris en charge pour l'installation de logiciels.\nVeuillez choisir un autre emplacement d'installation."},
        {"it", "'{0}' non è una posizione supportata per l'installazione del software.\nScegliere un'altra posizione di installazione."},
        {"ja", "{0}はソフトウェアをインストールするためにサポートされている場所ではない。\n他のインストール先を選択してください。"},
        {"nl", "'{0}' is geen ondersteunde locatie voor het installeren van software.\nKies een andere installatielocatie."},
        {"pl", "'{0}' nie jest obsługiwaną lokalizacją do instalacji oprogramowania.\nProszę wybrać inną lokalizację instalacji."},
        {"pt-BR", "'{0}' não é um local suportado para instalação de software.\nPor favor, escolha um local de instalação diferente."},
        {"pt-PT", "'{0}' não é uma localização suportada para a instalação de software.\nPor favor, escolha um local de instalação diferente."},
        {"ru", "'{0}' не является поддерживаемым местом для установки программного обеспечения.\nПожалуйста, выберите другое место установки."},
        {"zh", "{0}不是一个支持安装软件的位置。\n请选择另一个安装地点。"}
    }.GetBestLanguage(CultureInfo.CurrentUICulture)!;

    public static string FolderNotEmpty => new LocalizableStringCollection
    {
        {"en", "'{0}' is already in use.\nWe recommend you use an empty folder."},
        {"de", "'{0}' wird bereits verwendet.\nWir empfehlen Ihnen, einen leeren Ordner zu verwenden."},
        {"es", "'{0}' ya está en uso.\nLe recomendamos que utilice una carpeta vacía."},
        {"fr", "'{0}' déjà utilisé.\nNous vous recommandons d'utiliser un dossier vide."},
        {"it", "'{0}' è già in uso.\nSi consiglia di utilizzare una cartella vuota."},
        {"ja", "{0}は使用中です\n空のフォルダーを使用することをお勧めします。"},
        {"nl", "De map '{0}' is al in gebruik.\nWe raden je aan een lege map te gebruiken."},
        {"pl", "Folder '{0}' jest już w użyciu.\nZalecamy użycie pustego folderu."},
        {"pt-BR", "A pasta '{0}' já está sendo utilizada.\nRecomendamos que você use uma pasta vazia."},
        {"pt-PT", "'{0}' já está a ser utilizado.\nRecomendamos que utilize uma pasta vazia."},
        {"ru", "'{0}' уже используется.\nМы рекомендуем использовать пустую папку."},
        {"zh", "{0}已在使用。\n我们建议你使用一个空文件夹。"}
    }.GetBestLanguage(CultureInfo.CurrentUICulture)!;

    public static string UseAnyway => new LocalizableStringCollection
    {
        {"en", "Continue anyway"},
        {"de", "Trotzdem fortfahren"},
        {"es", "Continuar de todos modos"},
        {"fr", "Continuer quand même"},
        {"it", "Continua comunque"},
        {"ja", "続行する"},
        {"nl", "Toch doorgaan"},
        {"pl", "Kontynuuj pomimo to"},
        {"pt", "Continuar mesmo assim"},
        {"ru", "Все равно продолжить"},
        {"zh", "仍然继续"}
    }.GetBestLanguage(CultureInfo.CurrentUICulture)!;

    public static string ChooseDifferent => new LocalizableStringCollection
    {
        {"en", "Choose another one"},
        {"de", "Anderen Speicherort wählen"},
        {"es", "Seleccionar otra"},
        {"fr", "Choisir un autre emplacement"},
        {"it", "Scegline un altro"},
        {"ja", "別の場所を選ぶ"},
        {"nl", "Andere map selecteren"},
        {"pl", "Wybierz inny"},
        {"pt-BR", "Selecionar outro local"},
        {"pt-PT", "Escolher outro local"},
        {"ru", "Выбрать другую папку"},
        {"zh", "选择其他位置"}
    }.GetBestLanguage(CultureInfo.CurrentUICulture)!;

    public static string SpaceRequired => new LocalizableStringCollection
    {
        {"en", "Space required: ~{0}"},
        {"de", "Erforderlicher Speicherplatz: ~{0}"},
        {"es", "Espacio necesario: ~{0}"},
        {"fr", "Espace nécessaire: environ {0}"},
        {"it", "Spazio necessario: ~{0}"},
        {"ja", "必要な容量：~{0}"},
        {"nl", "Vereiste ruimte: ~{0}"},
        {"pl", "Wymagane miejsce na dysku: ~{0}"},
        {"pt-BR", "Espaço necessário: ~{0}"},
        {"pt-PT", "Espaço necessário: ~{0}"},
        {"ru", "Требуется места: ~{0}"},
        {"zh", "占用空间：~{0}"}
    }.GetBestLanguage(CultureInfo.CurrentUICulture)!;

    public static string SpaceAvailable => new LocalizableStringCollection
    {
        {"en", "Space available: {0}"},
        {"de", "Verfügbarer Speicherplatz: {0}"},
        {"es", "Espacio disponible: {0}"},
        {"fr", "Espace disponible: {0}"},
        {"it", "Spazio disponibile: {0}"},
        {"ja", "空き容量：{0}"},
        {"nl", "Beschikbare ruimte: {0}"},
        {"pl", "Dostępne miejsce na dysku: {0}"},
        {"pt-BR", "Espaço disponível: {0}"},
        {"pt-PT", "Espaço disponível: {0}"},
        {"ru", "Доступно: {0}"},
        {"zh", "可用空间：{0}"}
    }.GetBestLanguage(CultureInfo.CurrentUICulture)!;
}
