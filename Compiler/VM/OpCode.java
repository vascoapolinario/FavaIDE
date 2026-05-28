package VM;

public enum OpCode {
    iconst (0, 1),
    dconst (1, 1),
    sconst (2, 1),

    iprint (3, 0),
    iuminus(4, 0),
    iadd   (5, 0),
    isub   (6, 0),
    imult  (7, 0),
    idiv   (8, 0),
    imod   (9, 0),
    ieq    (10, 0),
    ineq   (11, 0),
    ilt    (12, 0),
    ileq   (13, 0),
    itod   (14, 0),
    itos   (15, 0),

    dprint (16, 0),
    duminus(17, 0),
    dadd   (18, 0),
    dsub   (19, 0),
    dmult  (20, 0),
    ddiv   (21, 0),
    deq    (22, 0),
    dneq   (23, 0),
    dlt    (24, 0),
    dleq   (25, 0),
    dtos   (26, 0),

    sprint (27, 0),
    sconcat(28, 0),
    seq    (29, 0),
    sneq   (30, 0),

    tconst (31, 0),
    fconst (32, 0),
    bprint (33, 0),
    beq    (34, 0),
    bneq   (35, 0),
    and    (36, 0),
    or     (37, 0),
    not    (38, 0),
    btos   (39, 0),

    halt   (40, 0),

    jump   (41, 1),
    jumpf  (42, 1),
    galloc (43, 1),
    gload  (44, 1),
    gstore (45, 1),

    lalloc (46, 1),
    lload  (47, 1),
    lstore (48, 1),
    pop    (49, 1),
    call   (50, 1),
    retval (51, 1),
    ret    (52, 1),

    aalloc (53, 0),
    aload  (54, 0),
    astore (55, 0),
    alength(56, 0),

    iread  (57, 0),
    dread  (58, 0),
    sread  (59, 0),
    bread  (60, 0),
    slength(61, 0);

    private final int code;
    private final int nArgs;

    OpCode(int code, int nArgs) {
        this.code = code;
        this.nArgs = nArgs;
    }

    public int code() {
        return code;
    }

    public int nArgs() {
        return nArgs;
    }

    public static OpCode convert(byte value) {
        int unsigned = Byte.toUnsignedInt(value);
        for (OpCode op : OpCode.values()) {
            if (op.code == unsigned) {
                return op;
            }
        }
        throw new IllegalArgumentException("Unknown opcode byte: " + unsigned);
    }
}
