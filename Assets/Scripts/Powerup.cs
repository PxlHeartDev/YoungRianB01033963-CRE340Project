using UnityEngine;
using UnityEngine.UI;

public class Powerup
{
    public string powerupName;
    public Image image;

    public int scoreValue = 3;

    public enum Effect
    {
        Heal,
        Speed,
    }

    public Effect effect;

    public Powerup()
    {
        scoreValue = Random.Range(3, 10);
        if (Random.value < 0.5f)
            effect = Effect.Heal;
        else
            effect = Effect.Speed;
    }


    public void Used(GameObject source, GameObject target = null)
    {
        Car car = source.transform.parent.gameObject.GetComponent<Car>();

        switch (effect)
        {
            case Effect.Heal:
                car.Heal();
                break;
            case Effect.Speed:
                car.SpeedBoost();
                break;
        }
    }
}
