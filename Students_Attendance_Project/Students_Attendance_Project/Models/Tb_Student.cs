
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
    
public partial class Tb_Student
{

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
    public Tb_Student()
    {

        this.Tb_GroupTask = new HashSet<Tb_GroupTask>();

        this.Tb_SingleTask = new HashSet<Tb_SingleTask>();

        this.Tb_StudentCheck = new HashSet<Tb_StudentCheck>();

    }


    public int StdID { get; set; }

    public string StdCode { get; set; }

    public string NameTH { get; set; }

    public string NameEN { get; set; }

    public int StudyGroupID { get; set; }

    public Nullable<int> StatusID { get; set; }



    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]

    public virtual ICollection<Tb_GroupTask> Tb_GroupTask { get; set; }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]

    public virtual ICollection<Tb_SingleTask> Tb_SingleTask { get; set; }

    public virtual Tb_Status Tb_Status { get; set; }

    public virtual Tb_StudyGroup Tb_StudyGroup { get; set; }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]

    public virtual ICollection<Tb_StudentCheck> Tb_StudentCheck { get; set; }

}

}
