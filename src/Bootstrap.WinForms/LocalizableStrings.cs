// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

namespace ZeroInstall;

public static class LocalizableStrings
{
    public static string Title => new LocalizableStringCollection
    {
        {"en", "{0}"}
    }.GetBestLanguage(CultureInfo.CurrentUICulture)!;

    public static string Continue => new LocalizableStringCollection
    {
        {"en", "Install"},
        {"de", "Installieren"},
        {"es", "Instalar"},
        {"fr", "Installer"},
        {"it", "Installa"},
        {"ja", "インストール"},
        {"nl", "Installeren"},
        {"pl", "Zainstaluj"},
        {"pt", "Instalar"},
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
        {"en", "Install here:"},
        {"de", "Hier installieren:"},
        {"es", "Instalar aquí:"},
        {"fr", "Emplacement:"},
        {"it", "Installa qui:"},
        {"ja", "インストール先"},
        {"nl", "{0} hier installeren:"},
        {"pl", "Zainstaluj {0} tutaj:"},
        {"pt", "Instalar aqui:"},
        {"ru", "Папка для установки:"},
        {"zh", "安装位置："}
    }.GetBestLanguage(CultureInfo.CurrentUICulture)!;

    public static string Change => new LocalizableStringCollection
    {
        {"en", "Change"},
        {"de", "Ändern"},
        {"es", "Cambiar"},
        {"fr", "Modifier"},
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

    public static string FolderNotEmpty => new LocalizableStringCollection
    {
        {"en", "'{0}' is already in use.\nWe recommend you choose another installation location."},
        {"de", "'{0}' wird bereits verwendet.\nWir empfehlen Ihnen, einen anderen Installationsort zu wählen."},
        {"es", "'{0}' ya está en uso.\nTe recomendamos cambiar la ubicación de la instalación."},
        {"fr", "'{0}' déjà utilisé.\nNous vous conseillons d’installer dans un autre emplacement."},
        {"it", "'{0}' è già in uso.\nSi consiglia di installare l'applicazione in un altro percorso."},
        {"ja", "{0}は使用中です\n別の場所へのインストールが推奨されます。"},
        {"nl", "De map '{0}' is al in gebruik.\nWij raden u aan om in een andere map te installeren."},
        {"pl", "Folder '{0}' jest już w użyciu.\nZalecamy zainstalowanie w innym folderze."},
        {"pt-BR", "A pasta '{0}' já está sendo utilizada.\nRecomendamos que você selecione outro local de instalação."},
        {"pt-PT", "'{0}' já está a ser utilizado.\nRecomendamos que escolha outro local de instalação."},
        {"ru", "'{0}' уже используется.\nМы рекомендуем вам выбрать другую папку для установки."},
        {"zh", "{0}已在使用。\n我们建议你选择其他安装位置。"}
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
}
