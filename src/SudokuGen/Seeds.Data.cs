// Seed data copied once from petewritescode/sudoku-gen (MIT, Copyright (c) Pete Williams): src/constants/seeds.constant.ts
// Cells use tokens 'a'-'i' for digits and '-' for an empty cell.
// Maintained by hand: seed order is part of SudokuId, so only append (see CODING_STANDARDS.md).
namespace SudokuGen;

internal static partial class Seeds
{
    internal static readonly Seed[] Easy =
    [
        new("g--d--caf---g----ii-f--hg-bb-iaedhgc--afcg--d-g-b-----f-d--abc---b------c--h-bfia", "gbhdiecafacegbfdhiidfcahgebbfiaedhgcehafcgibddgcbhiafefidegabchhabifcedgceghdbfia"),
        new("bf-hiac-g-gi------a-hf-g---g-a-fi--ddef---i-b--b-a-g-ff---gbh--hac---------e-cfd-", "bfdhiacegegicbdafhachfegdbighabfiecddefgchiabcibdaeghffdeagbhichacidfbgeibgehcfda"),
        new("hgad-e--b-cbf-ge---df-aih-----i-------d-ecai-g---fa----igadf----fe-i-----h-eg-fd-", "hgadceifbicbfhgeadedfbaihcgcahibdgeffbdgecaihgeihfadbcbigadfchedfecihbgaahcegbfdi"),
        new("-fbe-c----e-----a---g-ihb--gb-fhdc-eid-g-eahbch-----f-----ef-ga-g----e-i--hi-----", "afbegcidhheidfbgacdcgaihbefgbafhdcieidfgceahbchebaidfgbidcefhgafgchdaebieahibgfcd"),
        new("c--d-fgeb---g--i-hg-ih--da-a-g-b-cde-edc--a--b--------i-e-cd-ha-fb-h-e-ch--e-----", "cahdifgebedfgabichgbihecdafahgfbicdefedcghabibicadehfgigebcdfhadfbihaegchcaefgbid"),
        new("bi---ec--eg--h-fbdf--------i-hba-dfe----ehbig--bf-d-h--f-e-a-c-----g-e--cde--f--a", "bidgfecahegcahifbdfhadcbgeiichbagdfedafcehbiggebfidahchfgedaicbabihgcedfcdeibfhga"),
        new("-----ef-ha--bf--ecfe-gc---a----gbch--a--df-b--bi----f-h-af-gidbdf----g--i--c--ha-", "bicdaefghahgbfidecfedgchbiaedfagbchicahidfebggbiehcafdhcafegidbdfbhiagceigecbdhaf"),
        new("--fg--hec-ebc-------h-dfgabb--h-a-fg-g-df-i--f-a---b--hf----ad---if----hc-ea---bi", "dafgbihecgebcahdifichedfgabbidhcaefgegcdfbihafhaigebcdhfgbicadeabifedcghcdeahgfbi"),
        new("-----b-f-e-aih----bi----a----e---i---g-bf--a-----cihg-ic-fdhg-a--h---f-cgef-iad-b", "dhcgabefiefaihcbdgbigdefachcaehgdibfhgibfecadfbdacihgeicbfdhgeaadhebgficgefciadhb"),
        new("e--f-b-------eid-f--h----b-ge-c-fadhab-ihgfe-hc--d----d-g---cf---eg--h-bf---i----", "edcfgbihabgaheidcfifhdcaebggeicbfadhabdihgfechcfadebgidigbahcfecaegfdhibfhbeicgad"),
    ];

    internal static readonly Seed[] Medium =
    [
        new("g-hedcf---i-f--a--e--a-----c--i-deh-i-------g--g--e---a----f--c-cf-e-gi-b-------e", "gahedcfbidicfbgaehefbaihcgdcbaigdehfihebfadcgfdghceiabaeighfbdchcfdebgiabgdcaihfe"),
        new("-di--ac---b-cid-h---h--b-d-----f----h-d----fca---c-i--d----i-e-bh---cd-g-g---fac-", "fdighacbeebgcidfhacahfebgdigecifhbadhidabgefcafbdceighdcabgihefbhfeacdigigehdfacb"),
        new("--ac-i------ah-d---e----i---a-e-bc----g--f--ad---gae--ig-fa------hd-e-g-c-d-b----", "hdaceigfbbifahgdcegecbfdiahfaiedbchgehgicfbdadcbhgaeifigefachbdabhdiefgccfdgbhaei"),
        new("fg----i---h--f-e--e-bd--afh-f--hg--ic------b----f-c-----c-------eiac-gdf-b-----e-", "fgaebhicdihdcfaegbecbdgiafhdfebhgcaicahidefbgbigfacdhegdchefbiaheiacbgdfabfgidhec"),
        new("--d-g-fi---e-ci-d-a----eg-----i---f---bg--ec-e--d--haig----f----ha--------ch-g-e-", "cbdaghfiehgefciadbaifbdeghcdahiecbfgifbghaecdecgdfbhaigeicafdbhbhaeidcgffdchbgiea"),
        new("----d-a---a-ie---di------h-d-e--cg-b-b-e--i----c-i--dh--h-gf--c------b-g--i-ce-a-", "ehfcdgabicabiehfgdigdfbachediehacgfbhbgefdicaafcgibedhbehagfdicfcadhibeggdibcehaf"),
        new("---cfa-ibf---i-------g---f--i--h-cd-gdf--------cd--fb-------bc--gb---dhi---he--g-", "dhecfagibfbgeidhaccaigbhefdbiafhecdggdfbaciehhecdgifbaafhidgbceegbacfdhiicdhebagf"),
        new("a------g-b--di-a-f--e--ahi----a------bae--------ichbaei---de------c-igd-d-h----ci", "aidhefcgbbhgdicaeffcebgahidheiabgdfccbaefdihggdfichbaeiacgdefbhefbchigdadghfabeci"),
        new("----g-------ci--bg-i-de-af-------beh-----fgdi---eb-f----ah--ig---hg-d---cd--a----", "hacfgbdieefdciahbggibdehafcagfidcbehbceahfgdidhiebgfcafbahceigdiehgfdcabcdgbaiehf"),
        new("gfbc---dh-a-------d--a--fi--daifc--ech------f-------c-f---e--b---d-----i--igh-d--", "gfbcieadhiahbdfcegdceaghfibbdaifcghechgebdiafeifhagbcdfgcdeihbahbdfcaegiaeighbdfc"),
    ];

    internal static readonly Seed[] Hard =
    [
        new("-e-fh--a-g----ed---a--b-f---ih----dc--------a----g----b---i---dhc-gf-----g------e", "debfhciagghfiaedcbcaidbgfehaihbefgdcfbgcdiehaedchgabifbfaeihcgdhcegfdabiigdacbhfe"),
        new("----i-b---fc--a-h-eb----i-fcieg--ad---hd-e----d--a----f---b-e-i-------b--h--e----", "hageifbcdifcbdagheebdchgiafciegfbadhaghdcefibbdfiahcegfcahbdegideifgchbaghbaeidfc"),
        new("-------hg-----h-d-a-g---ei--ce--dg--dbf---------bfid--hg---f----d--h---c--a-eg---", "bedfiachgficeghbdaahgdbceificehadgfbdbfgceiahgahbfidcehgbcdfaeiediahbfgccfaieghbd"),
        new("h---f------------i--e---a-h-dhe---a---fh-b----i--c---gf-ga-di--a-i---d-bce------a", "hgcifabdedabgehfciifebdcaghbdheigcafgcfhabeideiadcfhbgfbgahdiecahicgedfbcedfbigha"),
        new("f----dha----b------a------dic---h------c--egb-----------a-----ed--f-ec-g-fg------", "febigdhachdcbfageigaiehcbfdicegbhadfahfcdiegbbgdaefichcbadigfhedihfaecbgefghcbdia"),
        new("c-a---i---b--c--ede----g--c-e---dga--c---b--i--gf-----b-----ei------a-cg--ie----a", "cfahdeigbgbhacifedeidbfgahchebcidgafacfgebhdiidgfahcbebacdgfeihfheibadcgdgiehcbfa"),
        new("--a-i---cc-g-------h--e--a--a---ib---d--f--h-----------i---d-f------g-c-dg---b--h", "beagifhdccfghdaibeihdbecfaghafcgibedgdbafechiecidbhagfaihecdgfbfbeihgdcadgcfabeih"),
        new("i--f--ec------a-fbg-b-i---h-d---ihg-----b---fe---a------d-----i---ie-b-------g---", "iahfdbecgdcehgaifbgfbeicdahbdacfihgechgdbeaifeifgahcbdhbdacfgeifgciedbhaaeibhgfdc"),
        new("--e---c------i--g-------d-hbaf--------cfhe--ie------f-h-d-c-----f-h----c---i-ga--", "fdegbhciaacheidfgbibgcfadehbafdgihcedgcfhebaiehibacgfdhidacfebggfahebidccebidgahf"),
        new("--f-d-i---g--b-a-d--c-a-----c-i---e---eh--g---------ac---------b---i-e----gf--d--", "abfcdhigehgiebfacddecgaibhfgcdifahebfaehcbgdiihbdegfacefabgdcihbdhaicefgcigfhedba"),
    ];

    internal static readonly Seed[] Expert =
    [
        new("-ica------------bh----g--f--g---a---i-e----c-a---f------d--bg------c---e-fg----id", "ficabhdeggeaidfcbhdhbegcifabgfceahdiidebhgacfachdfiegbeadfibghchbigcdfaecfghaebid"),
        new("-h-i------i---------f--bh--b---a--ed-ca------i--f---h--------c----he--f-ab--df---", "ehbicdgafdigafhcbecafegbhdibghcaifedfcadheigbiedfbgahchfebiadcggdihecbfaabcgdfeih"),
        new("-h--c-f-ice-------b--ia--------g-h------e---ff--h---i----b---eh----------ga--f--c", "ahgdcefbiceigfbahdbfdiahcgediefgchabgbhaeidcffachbdeigicfbdagehedbchgifahgaeifbdc"),
        new("a----db---g-c----f--e-f--i---------i----h-f-d--g---ch---b--e-c-ca------h-d-------", "afchidbegigdcebhafhbegfadicfehdgcabibcaehifgddigbafcheghbfdeicacafibgedhediachgfb"),
        new("------c--g-b--a---------g-h---e----gb--id-----i-f---eb----i---c-he-f-d--a------h-", "edhbgicfagfbchaeidicadefgbhhafebcidgbegidhacfdicfaghebfgdhiebaccheafbdgiabigcdfhe"),
        new("---bf-i-------hc-aa----------g------h--c-e----i----bh----f---g--f-----e---hig-a--", "chebfaidgfgdeihcbaabidcgefhbeghdifachafcbegiddicgafbheicafedhgbgfbahcdeiedhigbacf"),
        new("--c-----d---g-i--h-i----b--ace------d--bh----b--f---------e---------bea--d--a--c-", "gecabhifdfbagdicehhidefcbgaaceigdhbfdgfbheaicbhifcagdeiagcefdhbcfhdibeagedbhagfci"),
        new("-----d-h--h-----a-gb------i-----a--g----eh-c--i--d-----ge---a--d----f-----ab--i--", "iacefdghbehdgibcafgbfhacedicehfbadigfdgiehbcaaibcdgfehbgedhiafcdciagfhbehfabceigd"),
        new("-bi-------c----e---------af---eba-----a-i-g------c--i----h-e--d-e------gc-b--f---", "fbiaegdhcachdfbegiedgchibafgicebafdhbhafidgcedfegchaibiafhgecbdhedbacifgcgbidfhea"),
        new("---i--h-bc----b----g----a----gd-----e--h-f------b---ac-c------ha-----id--i--gd---", "deficahgbchagfbdeibgiedhacffagdicbheebchafgididhbegfacgcdabiefhafbcheidghiefgdcba"),
    ];

}
