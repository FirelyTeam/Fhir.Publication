```
var client = new FhirClient("http://spark.furore.com/fhir");

var pat = client.Read<Patient>("Patient/1");
pat.Resource.Name.Add(HumanName.ForFamily("Kramer")
        .WithGiven("Ewout"));

client.Update<Patient>(pat);
```

This library provides:

* Class models for working with the FHIR data model using POCO's
* Xml and Json parsers and serializers
* Validation functionality for instances created with the model classes
* A REST client for working with FHIR-compliant servers

We'll soon be adding handling of ValueSets and Profile validation.

picture: 
![logo]

### What's new?
The FHIR client is still under development. Check out [what's new](whats-new.html) in this latest release.

### Get Started
Get started by reading the [online documentation](docu-index.html), downloading the [NuGet package][2] or getting [the sourcecode][3].

```javascript
var s = "JavaScript syntax highlighting";
alert(s);
```

[![IMAGE ALT TEXT HERE](http://img.youtube.com/vi/SKHUdiLcC0w/0.jpg)](http://www.youtube.com/watch?v=SKHUdiLcC0w)


[1]: http://www.hl7.org/fhir
[2]: http://www.nuget.org/packages/Hl7.Fhir
[3]: http://www.github.com/ewoutkramer/fhir-net-api
[logo]: fhir.png
