import { TextInput } from "@mantine/core";
import { Form } from "@remix-run/react";
import { Search } from "tabler-icons-react";

export default function SearchBar() {
    return (                  
        <Form action={"/home/search"}>
            <TextInput name="title" placeholder={"Search"}  icon={<Search size={24} />} />
        </Form>
    );
}