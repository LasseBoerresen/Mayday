using mayday;
using mayday.Structures;

namespace Test.TestStructures;

public class FakeMaydayStructure() : MaydayStructure(
    joints: new Joint[] { },
    attachments: new Attachment[] { },
    links: new Link[] { });
