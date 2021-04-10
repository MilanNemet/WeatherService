# WeatherService
### Linux ismeretek (GKNB_MSTM028)
##### A csomag tartalma:
A program telep�t�je k�t f�jlb�l �ll:
1. setup.sh
2. **wsmra**.zip (**w**eather **s**ervice **m**ulti **r**elease **a**rchive)
##### A program telep�t�se:
A telep�t�shez a setup.sh f�jlnak futtat�si jogosults�got kell adni, p�ld�ul parancssorb�l a `chmod 700 setup.sh` paranccsal.
Ezut�n telep�thet� a program a k�vetkez�k�ppen:
1. a setup.sh futtat�s�val
    * parancssorb�l, a tartalmaz� mapp�b�l ind�tva a programot:
        * `./setup.sh`
        * `./setup.sh /abszol�t/el�r�si/�tvonal/...`
    * kattint�ssal, majd a "V�grehajt�s termin�lban" opci� v�laszt�s�val
3. a program alap�rtelmezetten az aktu�lis felhaszn�l� asztal�ra telep�l a _WService_ nev� mapp�ba
4. b�rhogy ind�tottuk is el a telep�t�st, a folyamat elej�n egyszer m�g j�v� kell hagyni a telep�t�si �tvonalat, illetve ekkor utolj�ra lehet�s�g ny�lik m�dos�tani is azt, **abszol�t** el�r�si �tvonal megad�s�val
5. a telep�t� a megadott �tvonalon szerepl� mapp�kat l�trehozza, amennyiben azok m�g nem l�teznek, �s kicsomagolja az architekt�r�nak megfelel� verzi�t
6. amennyiben az aktu�lis architekt�ra nem t�mogatott, t�rli a kicsomagolt �sszes f�jlt, az utols�nak l�trehozott "sz�l�mapp�val" egy�tt, �s a telep�t�s megszakad
7. ha az architekt�ra t�mogatott, l�trehoz egy start.sh f�jlt a telep�tett program f�k�nyvt�r�ba
8. be�temezi `cron`-ban a program futtat�s�t minden 4. �r�ban

##### A program futtat�sa:
1. A program f�k�nyvt�r�ban a start.sh elind�t�s�val:
    * termin�lb�l: `./start.sh`
    * kattint�ssal, majd a "V�grehajt�s termin�lban" opci� v�laszt�s�val
2. a program elindul `-u` param�terrel "user m�dban":
    1. megny�lik a parancssor
    2. a program megpr�b�lja a k�p�nyeg.hu web API el�r�s�vel let�lteni a legfrissebbb adatokat
    3. ezeket let�rolja a merevlemezen f�jlban
    4. elk�sz�ti a GUI-hoz sz�ks�ges bemen� adatokat
    5. a parancssor log�zeneteken jelen�t meg a program fut�s�nak mindenkori �llapot�r�l
    6. ha hiba l�p fel, a program �jrapr�b�lkozik, alap�rtelmezetten �sszesen 3 futtat�st k�s�rel meg, 5 m�sodperces elt�r�sekkel
    7. a futtat�s v�g�n, ak�r sikeres volt a lefut�s, ak�r sikertelen, billenty�zet le�t�sre v�r a felhaszn�l�t�l, �gy id� �s lehet�s�g ny�lik a logok megvizsg�l�s�ra (kiz�r�lag "user m�dban")
3. billety�gomb le�t�se ut�n �j b�ng�sz�ablak ny�lik meg, ahol az adatok grafikonos vizualiz�ci�j�t l�thatjuk
4. a b�ng�sz� ablak bez�r�s�val a parancssor is bez�rul, a program fut�sa befejez�d�tt

##### A program konfigur�ci�ja �s egy�b tudnival�k:
A b�ng�sz�ben a grafikonok a z�ld sarokn�l fogva �tm�retezhet�k.

A program automatikus fut�sa sor�n is keletkeznek logok, ezek megtekinthet�k az `el�r�si_�tvonal/app/Logs` mapp�ban, a legt�bb sz�vegszerkeszt�vel j�l olvashat� form�ban. G�pi feldolgoz�suk t�bl�zatk�nt lehets�ges, tabul�torral tagolt (tsv) f�jlk�nt.

A program �ll�that� param�terei az `el�r�si_�tvonal/app/appsettings.json` f�jlban tal�lhat�ak, kulcs-�rt�k p�rok form�j�ban. Ezt a konfigur�ci�t a program minden futtat�s sor�n �jra "lebuildeli".
A kulcsokhoz tartoz� �rt�kek megv�ltoztat�s�val lehet a programot be(de legink�bb csak el)�ll�tani. A l�nyegesebb kulcsok, amiket potenci�lisan �rdemes lehet �t�ll�tani (term�szetesen saj�t felel�ss�gre):
* MainLoop.Delay: pr�b�lkoz�sok k�z�tti k�sleltet�s (m�sodperc)
* MainLoop.MaxTries: maxim�lis pr�b�lkoz�si k�s�rletek sz�ma (darab)
* WebService.Timeout: maxim�lis v�rakoz�si id� a szerver v�lasz�ra (m�sodperc)
* DataFilter.TargetRegion: az adatgy�jt�s c�lj�ul szolg�l� magyarorsz�gi r�gi� neve, unicode form�tumban
* ILogger.LogLevel: a log�zenetek r�szletess�g�re utal� filter

Ezeken k�v�l, parancssorban a `crontab -e` paranccsal, a programhoz tartoz� cron-bejegyz�s m�dos�t�sa lehets�ges, az automatikus fut�si gyakoris�g �ll�t�s�nak c�lj�b�l.
**Helytelen konfigur�ci� a program hib�s m�k�d�s�t, vagy ak�r m�k�d�sk�ptelens�g�t id�zheti el�!**

##### A program t�rl�se:
1. A telep�t�si �tvonalon a WService mappa rekurz�v t�rl�se
2. Parancssorban a `crontab -e` paranccsal, a programhoz tartoz� cron-bejegyz�s t�rl�se.
