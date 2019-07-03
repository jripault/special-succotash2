using System;
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Candidate : MonoBehaviour
{
    Element[] elements = new Element[2];
    static float yDefault = 12.5f;
    static float yUp = 13f;
    int orientation = 0;
    bool isMoving = false;
    // 0 : 1-2
    // 1 : 1/2
    // 2 : 2-1
    // 3 : 2/1

    // Start is called before the first frame update
    public void Destroy()
    {
      Destroy(this);
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    void FixedUpdate(){

      if (isMoving){
        bool moved=false;
        foreach (Element elem in elements){
          if (elem.getGoElement().transform.position.y > elem.getLinetarget()) {
            elem.getGoElement().transform.Translate(Vector3.down * 30 * Time.deltaTime, Space.World);
            elem.setElementMoving(true);
          }
          else {
            elem.getGoElement().transform.position = new Vector3(elem.getGoElement().transform.position.x, elem.getLinetarget(), 0);
            elem.setElementMoving(false);
          }
          moved = moved || elem.isElementMoving();
        }
        isMoving = moved;
        if (!isMoving){
          Debug.Log("--------------------End of submit----------------------");
          registerElementsInGrid();
          enabled=false;
          FindObjectOfType<Game>().SpawnNewCandidate();
          Destroy(this);

        }

        //Debug.Log("isMoving : " + isMoving);
      }

    }

    // Update is called once per frame
    void Update()
    {




      if (Input.GetKeyDown(KeyCode.Space)){rotateCandidate();}
      else if (Input.GetKeyDown(KeyCode.LeftArrow)){move("left");}
      else if (Input.GetKeyDown(KeyCode.RightArrow)){move("right");}
      else if (Input.GetKeyDown(KeyCode.DownArrow)){submit();}
    }

    public void init(){
      for (int i = 0; i < elements.Length; i++){
          elements[i] = newElement(i);
      }
    }

    Element newElement(int number){

      int randomElement = UnityEngine.Random.Range(0, 4);

      Element element = new Element(randomElement);
      float x = 3 + number;
      element.getGoElement().transform.position = new Vector3(x, yDefault, 0);
      //element.transform.localScale = new Vector3(0.5f, 0.5f, 0);
      return element;
    }

    void rotateCandidate() {

      float minX = 99;
      float newX0 = 0;
      float newY0 = 0;
      float newX1 = 0;
      float newY1 = 0;

      foreach (Element elem in elements){
        if (elem.getGoElement().transform.position.x<minX){
          minX=elem.getGoElement().transform.position.x;
        }
      }

      switch (orientation) {
        case 0: // 0 : 1-2
          newX0 = minX;
          newY0 = yUp;
          newX1 = minX;
          newY1 = yUp-1;
          orientation = 1;
          break;
        case 1: // 1 : 1/2
          if (minX==Game.nbCols-1) {
            minX=Game.nbCols-2;
          }
          newX0 = minX+1;
          newY0 = yDefault;
          newX1 = minX;
          newY1 = yDefault;
          orientation = 2;
          break;
        case 2: // 2 : 2-1
          newX0 = minX;
          newY0 = yUp-1;
          newX1 = minX;
          newY1 = yUp;
          orientation = 3;
          break;
        case 3: // 3 : 2/1
          if (minX==Game.nbCols-1) {
            minX=Game.nbCols-2;
          }
          newX0 = minX;
          newY0 = yDefault;
          newX1 = minX+1;
          newY1 = yDefault;
          orientation = 0;
          break;
        default :
          newX0 = 0;
          newY0 = yDefault;
          newX1 = 1;
          newY1 = yDefault;
          orientation = 0;
          break;

      }

      elements[0].getGoElement().transform.position = new Vector3(newX0,newY0,0);
      elements[1].getGoElement().transform.position = new Vector3(newX1,newY1,0);

    }
    void move(string direction) {
      string reverseDirection="left";
      float increment = 1;
      if (direction == "left") {
        increment = -1;
        reverseDirection="right";
      }
      for (int i = 0; i < elements.Length; i++){
          elements[i].getGoElement().transform.position += Vector3.right * increment;
      }

      //check if piece is out of bounds
      int maxCols = Game.nbCols;
      foreach (Element elem in elements){
        /// minus 0.5 because it is the size of the asset. its coord is the left side of it
        if ((elem.getGoElement().transform.position.x<0)||(elem.getGoElement().transform.position.x>index2position(maxCols)-1)){
          move(reverseDirection);

        }
      }




    }

    void submit(){


      int i=0;
      float[] yS = new float[elements.Length];
      bool differentY=false;
      foreach (Element elem in elements){
        float newY = elem.getGoElement().transform.position.y;
        if ((i!=0) && (newY != yS[i-1]) ){
          differentY = true;
        }
        yS[i]=newY;
        i++;
      }
      if (differentY){
        Array.Sort(yS);
      }



      foreach (Element elem in elements){
        //colonne
        elem.setColtarget((int)elem.getGoElement().transform.position.x);

        //ligne
        int lineTarget = FindObjectOfType<Game>().firstNullinCol(elem.getColtarget());
        if (differentY){
          lineTarget += Array.IndexOf(yS, elem.getGoElement().transform.position.y);
        }
        elem.setLinetarget(lineTarget);

      }
      isMoving = true;


    }

    int position2index(float pos){
      return (int)(pos);
    }
    float index2position(int index){
      return (index);
    }


    public void registerElementsInGrid(){
      foreach (Element elem in elements){
        elem.registerInGrid(true);
      }
      FindObjectOfType<Game>().analyseGrid();
    }

}


/*
  Objetct for the elements
*/


public class Element : MonoBehaviour
{
  GameObject goElement;
  int elemType; //50 is null
  int colTarget = 0;
  int lineTarget = 0;
  bool elementMoving=false;
  bool inTransform = false;


  // Start is called before the first frame update
  void Start()
  {
  }

  // Update is called once per frame
  void Update()
  {
      //transform.RotateAround(Vector3.zero, Vector3.up, 20 * Time.deltaTime);
      //Debug.Log("Call to upadte element");
  }

  public void init(){

  }

  public Element (int randomElement){
    goElement = (GameObject)Instantiate(Resources.Load<GameObject>("Prefabs/"+randomElement.ToString()));
    elemType = randomElement;
  }

  public Element (bool nullElement){
    //goElement = (GameObject)Instantiate(Resources.Load<GameObject>("Prefabs/"+randomElement.ToString()));
    elemType = 50;
  }

  public GameObject getGoElement (){
    return goElement;
  }

  public int getElemType (){
    return elemType;
  }

  public int getColtarget (){
    return colTarget;
  }
  public int getLinetarget (){
    return lineTarget;
  }
  public void setColtarget (int target){
    colTarget = target;
  }
  public void setLinetarget (int target){
    lineTarget = target;
  }

  public bool isElementMoving(){
    return elementMoving;
  }
  public void setElementMoving(bool moving){
    elementMoving = moving;
  }

  public void registerInGrid(bool analyse){
    //registerelement in Grid
    FindObjectOfType<Game>().addToGrid(this,false);
  }

  public bool isNull(){
    if (elemType==50){
      return true;
    }
    else {
      return false;
    }
  }

  public bool isInTransform(){
    return inTransform;
  }
  public void setInTransform(bool flag){
    inTransform=flag;
  }

  public void destroy(){
    Destroy(goElement);
  }

}
