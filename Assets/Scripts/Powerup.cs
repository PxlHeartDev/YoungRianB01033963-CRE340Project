using UnityEngine;
using UnityEngine.UI;

public class Powerup
{
    public string powerupName;
    public Image image;

    public int scoreValue = 3;

    public Powerup()
    {
        scoreValue = Random.Range(3, 10);
    }

    public void Used(GameObject source, GameObject target = null)
    {

    }
}
