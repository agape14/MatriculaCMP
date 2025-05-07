namespace MatriculaCMP.Shared
{
    public class PerfilMenu
    {
        public int Id { get; set; }
        public int PerfilId { get; set; }
        public int MenuId { get; set; }

        public Perfil Perfil { get; set; }
        public Menu Menu { get; set; }
    }
}
