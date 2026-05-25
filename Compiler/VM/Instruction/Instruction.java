package VM.Instruction;

import VM.OpCode;

import java.io.DataOutputStream;
import java.io.IOException;

public class Instruction {
    protected OpCode opc;

    public Instruction(OpCode opc) {
        this.opc = opc;
    }

    public OpCode getOpCode() {
        return opc;
    }

    public int nArgs() {
        return 0;
    }

    @Override
    public String toString() {
        return opc.toString();
    }

    public void writeTo(DataOutputStream out) throws IOException {
        out.writeByte(opc.code());
    }
}