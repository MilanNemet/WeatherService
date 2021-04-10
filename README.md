# WeatherService
### Linux ismeretek (GKNB_MSTM028)
##### A csomag tartalma:
A program telepítõje két fájlból áll:
1. setup.sh
2. **wsmra**.zip (**w**eather **s**ervice **m**ulti **r**elease **a**rchive)
##### A program telepítése:
A telepítéshez a setup.sh fájlnak futtatási jogosultságot kell adni, például parancssorból a `chmod 700 setup.sh` paranccsal.
Ezután telepíthetõ a program a következõképpen:
1. a setup.sh futtatásával
    * parancssorból, a tartalmazó mappából indítva a programot:
        * `./setup.sh`
        * `./setup.sh /abszolút/elérési/útvonal/...`
    * kattintással, majd a "Végrehajtás terminálban" opció választásával
3. a program alapértelmezetten az aktuális felhasználó asztalára települ a _WService_ nevû mappába
4. bárhogy indítottuk is el a telepítést, a folyamat elején egyszer még jóvá kell hagyni a telepítési útvonalat, illetve ekkor utoljára lehetõség nyílik módosítani is azt, **abszolút** elérési útvonal megadásával
5. a telepítõ a megadott útvonalon szereplõ mappákat létrehozza, amennyiben azok még nem léteznek, és kicsomagolja az architektúrának megfelelõ verziót
6. amennyiben az aktuális architektúra nem támogatott, törli a kicsomagolt összes fájlt, az utolsónak létrehozott "szülõmappával" együtt, és a telepítés megszakad
7. ha az architektúra támogatott, létrehoz egy start.sh fájlt a telepített program fõkönyvtárába
8. beütemezi `cron`-ban a program futtatását minden 4. órában

##### A program futtatása:
1. A program fõkönyvtárában a start.sh elindításával:
    * terminálból: `./start.sh`
    * kattintással, majd a "Végrehajtás terminálban" opció választásával
2. a program elindul `-u` paraméterrel "user módban":
    1. megnyílik a parancssor
    2. a program megpróbálja a köpönyeg.hu web API elérésével letölteni a legfrissebbb adatokat
    3. ezeket letárolja a merevlemezen fájlban
    4. elkészíti a GUI-hoz szükséges bemenõ adatokat
    5. a parancssor logüzeneteken jelenít meg a program futásának mindenkori állapotáról
    6. ha hiba lép fel, a program újrapróbálkozik, alapértelmezetten összesen 3 futtatást kísérel meg, 5 másodperces eltérésekkel
    7. a futtatás végén, akár sikeres volt a lefutás, akár sikertelen, billentyûzet leütésre vár a felhasználótól, így idõ és lehetõség nyílik a logok megvizsgálására (kizárólag "user módban")
3. billetyûgomb leütése után új böngészõablak nyílik meg, ahol az adatok grafikonos vizualizációját láthatjuk
4. a böngészõ ablak bezárásával a parancssor is bezárul, a program futása befejezõdött

##### A program konfigurációja és egyéb tudnivalók:
A böngészõben a grafikonok a zöld saroknál fogva átméretezhetõk.

A program automatikus futása során is keletkeznek logok, ezek megtekinthetõk az `elérési_útvonal/app/Logs` mappában, a legtöbb szövegszerkesztõvel jól olvasható formában. Gépi feldolgozásuk táblázatként lehetséges, tabulátorral tagolt (tsv) fájlként.

A program állítható paraméterei az `elérési_útvonal/app/appsettings.json` fájlban találhatóak, kulcs-érték párok formájában. Ezt a konfigurációt a program minden futtatás során újra "lebuildeli".
A kulcsokhoz tartozó értékek megváltoztatásával lehet a programot be(de leginkább csak el)állítani. A lényegesebb kulcsok, amiket potenciálisan érdemes lehet átállítani (természetesen saját felelõsségre):
* MainLoop.Delay: próbálkozások közötti késleltetés (másodperc)
* MainLoop.MaxTries: maximális próbálkozási kísérletek száma (darab)
* WebService.Timeout: maximális várakozási idõ a szerver válaszára (másodperc)
* DataFilter.TargetRegion: az adatgyûjtés céljául szolgáló magyarországi régió neve, unicode formátumban
* ILogger.LogLevel: a logüzenetek részletességére utaló filter

Ezeken kívül, parancssorban a `crontab -e` paranccsal, a programhoz tartozó cron-bejegyzés módosítása lehetséges, az automatikus futási gyakoriság állításának céljából.
**Helytelen konfiguráció a program hibás mûködését, vagy akár mûködésképtelenségét idézheti elõ!**

##### A program törlése:
1. A telepítési útvonalon a WService mappa rekurzív törlése
2. Parancssorban a `crontab -e` paranccsal, a programhoz tartozó cron-bejegyzés törlése.
