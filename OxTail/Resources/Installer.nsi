;-----------------------------------------------------------------
;
; ALWAYS EDIT WITH A PLAIN TEXT EDITOR 
;
;OxTail NSIS installation script
; Dan Beavon
; 2010.06.07
; 
; See http://nsis.sourceforge.net/Main_Page for packaging software
;-----------------------------------------------------------------

;--------------------------------
;Include Modern UI

  !include "MUI2.nsh"

;--------------------------------
;General

  ;Name and file
  Name "OxTail Version 1.0"
  OutFile "OxTail.exe"

  ;Default installation folder
  InstallDir "$PROGRAMFILES\OxTail"

  ;Request application privileges for Windows Vista
  RequestExecutionLevel user



;--------------------------------
;Interface Settings

  !define MUI_ABORTWARNING

;--------------------------------
;Pages
  
  !insertmacro MUI_PAGE_WELCOME
  
  !insertmacro MUI_PAGE_LICENSE "..\COPYING.txt"  
  !insertmacro MUI_PAGE_COMPONENTS
  !insertmacro MUI_PAGE_DIRECTORY
  !insertmacro MUI_PAGE_INSTFILES
  !insertmacro MUI_PAGE_FINISH

  !insertmacro MUI_UNPAGE_WELCOME
  !insertmacro MUI_UNPAGE_CONFIRM
  !insertmacro MUI_UNPAGE_INSTFILES
  !insertmacro MUI_UNPAGE_FINISH

;--------------------------------
;Languages

  !insertmacro MUI_LANGUAGE "English"
;--------------------------------
;Installer Sections

Section "Core OxTail Files" SecCoreFiles

  SectionIn RO

  SetOutPath "$INSTDIR"

  ;ADD YOUR OWN FILES HERE...

  File "..\bin\Release\OxTail.exe"
  File "..\bin\Release\OxTail.Controls.dll"			  
  File "..\bin\Release\OxTailHelpers.dll"			  
  File "..\bin\Release\OxTailLogic.dll"	  
  File "..\bin\Release\SavedExpression.xml"			  
  File "..\bin\Release\highlights.xml"
  File "..\COPYING.txt"
  File "..\bin\Release\OxTail.exe.config"

  ;Create uninstaller
  WriteUninstaller "$INSTDIR\Uninstall.exe"

SectionEnd

; Add in when language packs available
;SectionGroup "OxTail Language Files" SecLangFiles
  
;	Section "German de-DE"
	  	  
;	  CreateDirectory "$INSTDIR\Culture"
;	  SetOutPath "$INSTDIR\Culture"	  

;	  File "..\bin\Release\Culture\Culture_de-DE.dll"	

;	SectionEnd

;SectionGroupEnd

;--------------------------------
;Descriptions

  ;Language strings
  LangString DESC_SecDummy ${LANG_ENGLISH} "The OxTail core files."

  ;Assign language strings to sections
  !insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
    !insertmacro MUI_DESCRIPTION_TEXT ${SecCoreFiles} $(DESC_SecCoreFiles)
  !insertmacro MUI_FUNCTION_DESCRIPTION_END

;--------------------------------
;Uninstaller Section

Section "Uninstall"

  Delete "$INSTDIR\OxTail.exe"
  Delete "$INSTDIR\*.dll"			  
  Delete "$INSTDIR\*.xml"
  Delete "$INSTDIR\Uninstall.exe"
  Delete "$INSTDIR\COPYING.txt"
  Delete "$INSTDIR\OxTail.exe.config"
  

  Delete "$SMPROGRAMS\OxTail\Uninstall.lnk"
  Delete "$SMPROGRAMS\OxTail\OxTail.lnk"

  ; Add in when language packs available
  ;Delete "$INSTDIR\Culture\*.dll"
  ;RMDir "$INSTDIR\Culture"
  
  RMDir "$INSTDIR"
  RMDir "$SMPROGRAMS\OxTail"

SectionEnd

Section "Start Menu Shortcuts"

  CreateDirectory "$SMPROGRAMS\OxTail"
  CreateShortCut "$SMPROGRAMS\OxTail\Uninstall.lnk" "$INSTDIR\uninstall.exe" "" "$INSTDIR\uninstall.exe" 0
  CreateShortCut "$SMPROGRAMS\OxTail\OxTail.lnk" "$INSTDIR\OxTail.exe" "" "$INSTDIR\OxTail.exe" 0	
  
SectionEnd

