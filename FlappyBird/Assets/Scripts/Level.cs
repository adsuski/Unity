using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey;
using CodeMonkey.Utils;

public class Level : MonoBehaviour
{
    private const float POLOWA_WYSOKOSCI_KAMERY = 50f;
    private const float SZEROKOSC_RURY = 7.8f;
    private const float WYSOKOSC_KRAWEDZI = 3.75f;
    private const float PREDKOSC_RURY = 30f;
    private const float POZYCJA_X_USUWAJACA_RURE = -100f;
    private const float POZYCJA_X_SPAWNUJACA_RURE = +100f;
    private const float POZYCJA_X_USUWAJACA_PODLOGE = -200f;
    private const float POZYCJA_X_PTAKA = 0f;

    private static Level instance;

    public static Level GetInstance()
    {
        return instance;
    }

    private List<Transform> listaPodlog;
    private List<Rura> listaRur;
    private int licznikPokonanychRur;
    private int zespawnowaneRury;
    private float spawnRuryTimer;
    private float spawnRuryTimerMax;
    private float rozmiarLuki;
    private State state;


    public enum Difficulty
    {
        Easy,
        Medium,
        Hard,
        Impossible,
    }

    private enum State
    {
        Oczekiwanie,
        Gra,
        SmiercPtaka,
    }


    private void Awake()
    {
        instance = this;
        ZainicjowanieSpawnuPodlogi();
        listaRur = new List<Rura>();
        spawnRuryTimerMax = 1f;
        rozmiarLuki = 50f;
        ustawPoziomTrudnosci(Difficulty.Easy);
        state = State.Oczekiwanie;
    }

    private void Start()
    {
        //StworzRure(40f, 20f,true);
        //StworzRure(40f, 20f,false);
        //StworzRureWokolLuki(50f, 20f, 20f);
        Bird.GetInstance().Smierc += Bird_Smierc;
        Bird.GetInstance().RozpoczecieGry += Bird_RozpoczecieGry;
    }

    private void Bird_RozpoczecieGry(object sender, System.EventArgs e)
    {
        state = State.Gra;
    }


    private void Bird_Smierc(object sender, System.EventArgs e)
    {
        // CMDebug.TextPopupMouse("Dead!");
        state = State.SmiercPtaka;

      
    }

    private void Update()
    {
        if (state == State.Gra) { 
        poruszanieSieRur();
        spawningRur();
        funkcjaPodlogi();
        }
    }
    private void ZainicjowanieSpawnuPodlogi()
    {

        listaPodlog = new List<Transform>();
        // Spawnuje 3 "podlogi" jedna za druga 
        Transform przeksztalceniePodlogi;
        float podlogaY = -47.5f;
        float szerokoscPodlogi = 192f;
        przeksztalceniePodlogi = Instantiate(GameAssets.GetInstance().pfGround, new Vector3(0, podlogaY, 0), Quaternion.identity);
        listaPodlog.Add(przeksztalceniePodlogi);
        przeksztalceniePodlogi = Instantiate(GameAssets.GetInstance().pfGround, new Vector3(szerokoscPodlogi, podlogaY, 0), Quaternion.identity);
        listaPodlog.Add(przeksztalceniePodlogi);
        przeksztalceniePodlogi = Instantiate(GameAssets.GetInstance().pfGround, new Vector3(szerokoscPodlogi*2f, podlogaY, 0), Quaternion.identity);
        listaPodlog.Add(przeksztalceniePodlogi);
    }

    private void funkcjaPodlogi()
    {
        foreach(Transform przeksztalceniePodlogi in listaPodlog)
        {
            przeksztalceniePodlogi.position += new Vector3(-1, 0, 0) * PREDKOSC_RURY * Time.deltaTime;

            if (przeksztalceniePodlogi.position.x < POZYCJA_X_USUWAJACA_PODLOGE)
            { //znalezienie x najbardziej z prawej aby podloga uciekajaca z lewej strony sceny pojawila sie po prawej stronie
                float pozycjaXZPrawej = -100f;
                for (int i=0;i< listaPodlog.Count; i++)
                {
                    if (listaPodlog[i].position.x> pozycjaXZPrawej)
                    {
                        pozycjaXZPrawej = listaPodlog[i].position.x;
                    }
                }
                //ulokowanie podlogi na pozycji z prawej
                float szerokoscPodlogi = 192f;
                przeksztalceniePodlogi.position = new Vector3(pozycjaXZPrawej+ szerokoscPodlogi, przeksztalceniePodlogi.position.y, przeksztalceniePodlogi.position.z);
            }
        }
    }

    private void spawningRur()
    {
        spawnRuryTimer -= Time.deltaTime;
        if (spawnRuryTimer < 0)
        {
            //czas na zespawnowanie nowej rury
            spawnRuryTimer += spawnRuryTimerMax;
            float limitWysokosciKrawedzi = 10f;
            float minWysokosc = rozmiarLuki * .5f+ limitWysokosciKrawedzi;
            float calaWysokosc = POLOWA_WYSOKOSCI_KAMERY * 2f;
            float maksWysokosc = calaWysokosc - rozmiarLuki * .5f - limitWysokosciKrawedzi;

            float wysokosc = Random.Range(minWysokosc, maksWysokosc);
            StworzRureWokolLuki(wysokosc, rozmiarLuki, POZYCJA_X_SPAWNUJACA_RURE);
        }
    }

    private void poruszanieSieRur()
    {
        for(int i=0;i<listaRur.Count; i++)
        {
            Rura rura = listaRur[i];
            bool jestPoPrawejOdPtaka=rura.PobierzPozycjeX() > POZYCJA_X_PTAKA;
            rura.Ruch();
            if(jestPoPrawejOdPtaka && rura.PobierzPozycjeX() <= POZYCJA_X_PTAKA && rura.CzyDolna()) //rura wspolrzedna X jest juz po lewej stronie i gracz pokonal przeszkode a dzieki funkcji CzyDolna liczy tylko dolna rure do wyniku
            {
                licznikPokonanychRur++;
            }
            if (rura.PobierzPozycjeX() < POZYCJA_X_USUWAJACA_RURE)
            {
                rura.ZniszczenieObiektu();
                listaRur.Remove(rura);
                i--;
            }
        }
    }

    private void ustawPoziomTrudnosci(Difficulty poziomTrudnosci)
    {
        switch (poziomTrudnosci)
        {
            case Difficulty.Easy:
                rozmiarLuki = 50f;
                spawnRuryTimerMax = 1.2f;
                break;
            case Difficulty.Medium:
                rozmiarLuki = 42f;
                spawnRuryTimerMax = 1.1f;
                break;
            case Difficulty.Hard:
                rozmiarLuki = 34f;
                spawnRuryTimerMax = 1.0f;
                break;
            case Difficulty.Impossible:
                rozmiarLuki = 25f;
                spawnRuryTimerMax = .8f;
                break;
        }
    }

    private Difficulty pobierzPoziomTrudnosci()
    {
        if (zespawnowaneRury >= 30) return Difficulty.Impossible;
        if (zespawnowaneRury >= 20) return Difficulty.Hard;
        if (zespawnowaneRury >= 10) return Difficulty.Medium;
        return Difficulty.Easy;

    }


    private void StworzRureWokolLuki(float lukaY, float rozmiarLuki, float xPozycja)
    {

        StworzRure(lukaY - rozmiarLuki * .5f, xPozycja, true);
        StworzRure(POLOWA_WYSOKOSCI_KAMERY * 2f - lukaY - rozmiarLuki * .5f, xPozycja, false);
        zespawnowaneRury++;
        ustawPoziomTrudnosci(pobierzPoziomTrudnosci());
    }

    private void StworzRure(float wysokosc, float xPozycja, bool dolna)//dolna do sprawdzenia ilosci przebytych luk, bo kazde przejscie luki powoduje naliczenie dwoch rur- gornej i dolnej, a chce liczyc przejscie tylko jednej
    {
        //Ustawienie Krawedzi rury
        Transform Krawedz = Instantiate(GameAssets.GetInstance().pfKrawedz);

        float pozycjaYKrawedzi;
        if (dolna)
        {
            pozycjaYKrawedzi = -POLOWA_WYSOKOSCI_KAMERY + wysokosc - WYSOKOSC_KRAWEDZI * .5f;
        }
        else
        {
            pozycjaYKrawedzi = POLOWA_WYSOKOSCI_KAMERY - wysokosc + WYSOKOSC_KRAWEDZI * .5f;
        }
        Krawedz.position = new Vector3(xPozycja, pozycjaYKrawedzi);
        

        //Ustawienie samej rury bez krawedzi
        Transform RuraBezKrawedzi = Instantiate(GameAssets.GetInstance().pfRura);
        float pozycjaYRury;
        if (dolna)
        {
            pozycjaYRury = -POLOWA_WYSOKOSCI_KAMERY;
        }
        else
        {
            pozycjaYRury = POLOWA_WYSOKOSCI_KAMERY;
            RuraBezKrawedzi.localScale = new Vector3(1, -1, 1);
        }
        RuraBezKrawedzi.position = new Vector3(xPozycja, pozycjaYRury);
        

        SpriteRenderer ruraSpriteRenderer = RuraBezKrawedzi.GetComponent<SpriteRenderer>();
        ruraSpriteRenderer.size = new Vector2(SZEROKOSC_RURY, wysokosc);

        BoxCollider2D ruraBoxCollider = RuraBezKrawedzi.GetComponent<BoxCollider2D>();
        ruraBoxCollider.size = new Vector2(SZEROKOSC_RURY, wysokosc);
        ruraBoxCollider.offset = new Vector2(0f, wysokosc * .5f);


        Rura rura = new Rura(Krawedz, RuraBezKrawedzi, dolna);
        listaRur.Add(rura);
    }

    //storzenie jednej całej rury z krawedzia zewnetrzna

    private class Rura
    {
        private Transform przeksztalcenieRuryBezKrawedzi;
        private Transform przeksztalcenieKrawedzi;
        private bool czyDolna;

        public Rura( Transform przeksztalcenieKrawedzi, Transform przeksztalcenieRuryBezKrawedzi,bool czyDolna)
        {
            this.przeksztalcenieKrawedzi = przeksztalcenieKrawedzi;
            this.przeksztalcenieRuryBezKrawedzi = przeksztalcenieRuryBezKrawedzi;
            this.czyDolna = czyDolna;
        }

        public void Ruch()
        {
            przeksztalcenieKrawedzi.position += new Vector3(-1, 0, 0) * PREDKOSC_RURY * Time.deltaTime;
            przeksztalcenieRuryBezKrawedzi.position += new Vector3(-1, 0, 0) * PREDKOSC_RURY * Time.deltaTime;
            
        }
        public bool CzyDolna()
        {
            return czyDolna;
        }

        public float PobierzPozycjeX()
        {
            return przeksztalcenieKrawedzi.position.x;
        }

        public void ZniszczenieObiektu()
        {
            Destroy(przeksztalcenieKrawedzi.gameObject);
            Destroy(przeksztalcenieRuryBezKrawedzi.gameObject);
        }
    }
    public int pobierzLiczbeZespawnowanychRur()
    {
        return zespawnowaneRury;
    }
    public int pobierzLiczbePokonanychRur()
    {
        return licznikPokonanychRur;
    }
}
