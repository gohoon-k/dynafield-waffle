[System.Serializable]
class Chart {

    public Note[] chart;
    public Speed[] speed;
    public MoveY[] move;
    public MoveX[] move_x;
    public Zoom[] zoom;

    public float end_margin;

    [System.Serializable]
    public class Note{
        public float t;
        public short x;
        public short s;
        public byte ty;
        public sbyte d;
        
        /* Hold only */
        public float dur;
        
        /* Counter only */
        public float timeout;
        public int counts;
    }

    [System.Serializable]
    public class Speed {
        public float t;
        public float s;
    }

    [System.Serializable]
    public class MoveY {
        public float t;
        public int d;
        public float dur;
        public int i;
    }

    [System.Serializable]
    public class MoveX {
        public float t;
        public int d;
        public float dur;
        public byte i;
    }

    [System.Serializable]
    public class Zoom {
        public float t;
        public float d;
        public float dur;
        public byte i;
    }

}