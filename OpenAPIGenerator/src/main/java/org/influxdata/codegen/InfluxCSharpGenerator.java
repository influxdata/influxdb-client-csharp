package org.influxdata.codegen;

import javax.annotation.Nonnull;

import org.openapitools.codegen.languages.CSharpClientCodegen;

public class InfluxCSharpGenerator extends CSharpClientCodegen {

    /**
     * Configures a friendly name for the generator.  This will be used by the generator
     * to select the library with the -g flag.
     *
     * @return the friendly name for the generator
     */
    @Nonnull 
    public String getName() {
        return "influx-csharp";
    }

    /**
     * Returns human-friendly help for the generator.  Provide the consumer with help
     * tips, parameters here
     *
     * @return A string value for the help message
     */
    @Nonnull 
    public String getHelp() {
        return "Generates a influx-csharp client library.";
    }

    public InfluxCSharpGenerator() {
        super();

        embeddedTemplateDir = templateDir = "csharp";
    }
}