using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    //Grille de 10 lignes et 8 colonnes
    public static int nbLines = 11;
    public static int nbCols = 8;
    public static bool debug = false;
    public static bool debugAnalyse = true;

    public static bool hold = false;
    Element[,] grid = new Element[nbCols,nbLines];

    List<Element> movingElements = new List<Element>();

    // Start is called before the first frame update
    void Start()
    {
        //Initialisaizing empty Grid
        /* an array with 5 rows and 2 columns*/
       int i, j;
       for (i = 0; i < nbCols; i++) {
          for (j = 0; j < nbLines; j++) {
             grid[i,j]=new Element(true); //création d'éléments de type vide
          }
       }
      debugGrid();
      SpawnNewCandidate();

    }

    // Update is called once per frame
    void Update()
    {


        //Element descendants :
        List<Element> elementsToRemove = new List<Element>();
        if (!hold){
          foreach (Element elem in movingElements){
            try{
              if (elem.getGoElement().transform.position.y > elem.getLinetarget()) {
                elem.getGoElement().transform.Translate(Vector3.down * 20 * Time.deltaTime, Space.World);
                elem.setElementMoving(true);
              }
              else {
                elem.getGoElement().transform.position = new Vector3(elem.getGoElement().transform.position.x, elem.getLinetarget(), 0);
                elem.setElementMoving(false);
                elementsToRemove.Add(elem);
              }
            }
            catch (MissingReferenceException e){
              Debug.Log("[Update] trying to move a destroyed object");
              elementsToRemove.Add(elem);
            }

          }
          foreach (Element toRemoveElemant in elementsToRemove){
            movingElements.Remove(toRemoveElemant);
          }
        }


    }

    //Spawn new Candidate
    public void SpawnNewCandidate(){
      if (debug) Debug.Log("[SpawnNewCandidate]");
      GameObject newCandidate = (GameObject)Instantiate(Resources.Load<GameObject>("Prefabs/NewCandidate"));
      Candidate candidate = newCandidate.GetComponent(typeof(Candidate)) as Candidate;
      candidate.init();
    }

    public void SpawnNewCandidate(Candidate candidate){
      candidate.Destroy();
      SpawnNewCandidate();
    }


    public int firstNullinCol(int numcol){
      if (debug) Debug.Log("[firstNullinCol]");
      for (int i=0; i<nbLines;i++){
        Element elem = grid[numcol,i];
        if (elem.isNull()){
          return i;
        }
      }
      return -1;
    }

    public int firstNullinColWithMax(int numcol, int maxLine){
      if (debug) Debug.Log("[firstNullinCol]");
      for (int i=0; i<maxLine;i++){
        Element elem = grid[numcol,i];
        if (elem.isNull()){
          return i;
        }
      }
      return -1;

    }

    public void addToGrid (Element element,bool analyse){
      if (debug) Debug.Log("[addToGrid]");
      if (debug) Debug.Log("Registering : " + element.getElemType() + "Col: " + element.getColtarget() + "Line: " + element.getLinetarget());
      grid[element.getColtarget(),element.getLinetarget()] = element;
      if (analyse) {
        analyseGrid();
      }
    }

    public void analyseGrid(){
      if (debugAnalyse) Debug.Log("[analyseGrid]");
      List<Transformation> transforms = identifyTransforms();

      bool transformationChangedSomething =false;

      foreach (Transformation trans in transforms){
        bool transformationReturn = executeTransformation(trans);
        transformationChangedSomething = (transformationChangedSomething||transformationReturn);
      }

      if (transformationChangedSomething){
        if (debugAnalyse) Debug.Log("[analyseGrid] transformations Changed something, requesting fall and analyse");
        hold = true;
        fallGrid(); // on fait tout descendre
        //while(movingElements.Count!=0){
          //wait
        //}
        hold = false;
        analyseGrid();
      }

      //while (transformationChangedSomething){
      //  analyseGrid();
      //}



      //Gestion du Game Over
      for (int i=0; i<nbCols;i++){
        Element elem = grid[i,nbLines-1];
        if (!elem.isNull()){
            gameOver();
        }
      }
      //Debug.Log("[analyseGrid] Alright, line " + (nbLines-1) + " is all null");
      debugGrid();
    }


    public void gameOver(){
      Debug.Log("Game Over ;(");
      Application.LoadLevel("GameOver");
    }

    List<Transformation> identifyTransforms(){
      if (debugAnalyse) Debug.Log("[identifyTransforms]");
      List<Transformation> transforms = new List<Transformation>();

      for (int col=0;col<nbCols;col++){ //all colonnes
        for (int line=0;line<nbLines;line++){ //all lines

          if (debugAnalyse) Debug.Log("[identifyTransforms] Analyzing col : "+col+"/ line : "+line);
          Element elem = grid[col,line];
          if ((!elem.isInTransform())&&(!elem.isNull())) //element pas déjà identifié et pas null
          {
              List<Element> group = new List<Element>(); //on créé un group d'éléments
              group.AddRange(findMatchingNeighbors(elem,false)); // on ajoute les voisins similaires dans le groupe
              if (debugAnalyse) Debug.Log("[identifyTransforms] Creating new Transformation of " + group.Count + " elements");
              Transformation transfo = new Transformation(group); // initialise une transformation pour chaque group
              transforms.Add(transfo);
          }
          else {
            if (debugAnalyse) Debug.Log("[identifyTransforms] Element is Null or already in TRansform");
          }
        }
      }

      //foreach ()

      return transforms;
    }

    void fallGrid(){
      for (int col=0;col<nbCols;col++){ //pour chaque colonnes
        fallColonne(col);
      }
    }


    void fallColonne(int col){
      for (int line=0;line<nbLines;line++){ //all lines
        Element elem = grid[col,line];
        if (!elem.isNull()){
          int firstNull = firstNullinColWithMax(col,line); //retourne la première position nulle en desssous
          if (firstNull!=-1){
            //il y en a une
            elem.setLinetarget(firstNull);
            elem.registerInGrid(false);
            grid[col,line] = new Element(true); // on reinit le trou
            movingElements.Add(elem);
          }
        }
      }
    }

    List<Element> findMatchingNeighbors (Element elem, bool inSub){
      if (debugAnalyse) Debug.Log("[findMatchingNeighbors] inSub = " + inSub);
      List<Element> matchingNeighbors = new List<Element>();
      int col = elem.getColtarget();
      int line = elem.getLinetarget();

      if (!inSub) elem.setInTransform(true);
      //matchingNeighbors.Add(elem); --> à faire après la récursivité

      int[] gauche = {-1, 0};
      int[] droite = {1, 0};
      int[] haut = {0, +1};
      int[] bas = {0, -1};

      int[][] neighbors = {gauche, droite, haut, bas};

      for (int i=0;i<neighbors.Length;i++){
        try {
          Element neighbor = grid[col+neighbors[i][0],line+neighbors[i][1]];
          if ((!neighbor.isInTransform())&&(neighbor.getElemType()==elem.getElemType())){
            if (debugAnalyse) Debug.Log("[findMatchingNeighbors] Adding Matching Neightbor : " + neighbor.getColtarget() + " -  " + neighbor.getLinetarget());
            neighbor.setInTransform(true);
            matchingNeighbors.Add(neighbor);
          }
        }
        catch (System.IndexOutOfRangeException e) {
          if (debugAnalyse) Debug.Log("[findMatchingNeighbors] IndexOutOfRangeException");
        }
      }

      //recursif !!
      List<Element> subMatchingNeighbors = new List<Element>();
      foreach(Element neighbor in matchingNeighbors){
        subMatchingNeighbors.AddRange(findMatchingNeighbors(neighbor,true));
      }
      if (!inSub) matchingNeighbors.Add(elem); // on ajoute le premier à la fin sinon ca boucle !!. Si l'on est en récursif, l'élément est déjà dedand pas besoin de le remettre
      matchingNeighbors.AddRange(subMatchingNeighbors);
      return matchingNeighbors;

    }


    bool executeTransformation(Transformation trans){
      bool transformationChangedSomething = false;

      if (debugAnalyse) Debug.Log("[executeTransformation]");
      if (debugAnalyse)Debug.Log("[executeTransformation] Operation transformation, count : " +  trans.getCount());
      if (trans.getCount()>2){
        trans.destroy();
        transformationChangedSomething = true;
      }
      else {
        trans.release();
      }

      return transformationChangedSomething;

    }

    public void deleteFromGrid(int col, int line){
      if (debug) Debug.Log("[deleteFromGrid]");
      grid[col,line] = new Element(true);
    }


    public void debugGrid(){
      string debugMessage = "";
      for (int line=nbLines-1;line>=0;line--){
        debugMessage += "\r\n";
        for (int col=0;col<nbCols;col++){
          debugMessage +=  grid[col,line].getElemType() + " \t ";

        }
      }
      Debug.Log(debugMessage);
    }


}


public class Transformation : MonoBehaviour
{
  List<Element> elementGroup;

  public Transformation(List<Element> group){
    elementGroup = group;
  }

  public int getCount(){
      return elementGroup.Count;
  }

  public void destroy(){
    foreach (Element elem in elementGroup){
      elem.setInTransform(false);
      FindObjectOfType<Game>().deleteFromGrid(elem.getColtarget(),elem.getLinetarget());
      elem.destroy();
    }

  }

  public void release(){
    foreach (Element elem in elementGroup){
      elem.setInTransform(false);
    }

  }
}
