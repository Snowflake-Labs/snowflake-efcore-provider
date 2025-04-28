using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EFCore.Tests;

public class Animal
{
    [Key]
    public int AnimalId { get; set; }
    public string Name { get; set; }
    public string? Breed { get; set; }
    public int? Age { get; set; }
    public List<VetClinic> Clinics { get; set; } = new();
    
    private Animal()
    {
    }

    public Animal(int animalId, string name, string breed, int? age)
    {
        AnimalId = animalId;
        Name = name;
        Breed = breed;
        Age = age;
    }
 
    public override string ToString()
    {
        return $"Animal(id:{AnimalId}, name:{Name}, breed:{Breed}, age:{Age})";
    }   
}

public record VetClinic
{   
    [Key]
    public int VetClinicId { get; set; }

    public List<Animal> Animals { get; } = new();

    public string Name { get; set; }

    public VetClinic()
    {
    }

    public VetClinic(int vetClinicId, string name, List<Animal> animals)
    {
        VetClinicId = vetClinicId;
        Name = name;
        Animals = animals;
    }

    public override string ToString()
    {
        return $"VetClinic(id:{VetClinicId}, name:{Name}, animals:{Animals.Count})";
    }}

public class PetOwner
{
    [Key]
    public int PetOwnerId { get; set; }
    public List<Animal> Animals { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }


    public PetOwner()
    {
        Animals = new List<Animal>();
    }

    public PetOwner(int petOwnerId, string name, string surname, List<Animal> animals)
    {
        PetOwnerId = petOwnerId;
        Name = name;
        Surname = surname;
        Animals = animals;
    }

    public override string ToString()
    {
        return $"PetOwner(id:{PetOwnerId}, name:{Name}, surname:{Surname}, animals:{Animals.Count})";
    }
}

public record Parent(int ParentId, string Name);

public record Child(int ChildId, string Name, int ParentId);
