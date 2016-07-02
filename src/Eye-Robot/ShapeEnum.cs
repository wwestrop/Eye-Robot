namespace EyeRobot {

    internal class ShapeEnum {

        public static readonly ShapeEnum Circle = new ShapeEnum("Circle");
        public static readonly ShapeEnum Triangle = new ShapeEnum("Triangle");
        public static readonly ShapeEnum Square = new ShapeEnum("Square");
        public static readonly ShapeEnum Penatagon = new ShapeEnum("Penatagon");
        public static readonly ShapeEnum Star4 = new ShapeEnum("Four-pointed star");
        public static readonly ShapeEnum Star5 = new ShapeEnum("Five-pointed star");
        public static readonly ShapeEnum Star6 = new ShapeEnum("Six-pointed star");

        private readonly string _name;

        private ShapeEnum(string name) {
            this._name = name;
        }

        public override string ToString() {
            return this._name;
        }
    }

}
