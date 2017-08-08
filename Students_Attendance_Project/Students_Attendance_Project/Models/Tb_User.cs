//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Students_Attendance_Project.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Tb_User
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Tb_User()
        {
            this.Tb_Login = new HashSet<Tb_Login>();
            this.Tb_Schedule = new HashSet<Tb_Schedule>();
            this.Tb_StudyGroup = new HashSet<Tb_StudyGroup>();
        }
    
        public int UserID { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public string NameEN { get; set; }
        public Nullable<int> DeptCode { get; set; }
        public string Role { get; set; }
    
        public virtual Tb_Department Tb_Department { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Tb_Login> Tb_Login { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Tb_Schedule> Tb_Schedule { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Tb_StudyGroup> Tb_StudyGroup { get; set; }
    }
}
