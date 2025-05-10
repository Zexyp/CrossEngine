#set HEADER_NUM 1
#define EXAMPLE_HEADER(NAME; LOCATION)
### %HEADER_NUM) %NAME
Umístění: [`./%LOCATION`](./%LOCATION) ([Projekt](./%LOCATION/%LOCATION.ceproj))

#inc HEADER_NUM 1
#enddef

# Příklady
Ve složce s příkladem vždy naleznete projektový soubor `*.ceproj`.

#EXAMPLE_HEADER(Modely; Sponza)
Načítání modelu s materiály i texturami

#EXAMPLE_HEADER(Průhlednost; Transparency)
Řazení průhlednýh objektů

#EXAMPLE_HEADER(Světelné zdroje; Lights)
Ukázka světelných zdrojů

#EXAMPLE_HEADER(Materiály; Materials)
Více druhů materiálů

#EXAMPLE_HEADER(Světelné zdroje; Lights)
Ukázka světelných zdrojů

#EXAMPLE_HEADER(Textury; Container)
Materiál s více texturami

#EXAMPLE_HEADER(Skybox; Materials)
Pŕíklad pozadí lze najít v projektu s materiály

#EXAMPLE_HEADER(Pohyblivá textura; Scripting)
Pro spuštění potřeba využít tlačítka **start**

#EXAMPLE_HEADER(Mlha; Fog)
Využití exponencionální mlhy
